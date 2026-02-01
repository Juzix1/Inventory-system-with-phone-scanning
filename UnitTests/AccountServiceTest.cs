using Moq;
using Moq.EntityFrameworkCore;
using InventoryLibrary.Services;
using InventoryLibrary.Services.Interfaces;
using InventoryLibrary.Model.Accounts;
using InventoryLibrary.Data;
using MockQueryable.Moq;
using Microsoft.EntityFrameworkCore;


namespace InventorySystem.Tests.Services;

public class AccountsServiceTests : IDisposable
{
    private readonly MyDbContext _context;
    private readonly Mock<IPasswordService> _mockPasswordService;
    private readonly IAccountsService _service;
    private readonly Mock<IInventoryLogger<AccountsService>> _mockLogger;


    public AccountsServiceTests()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MyDbContext(options);
        _mockPasswordService = new Mock<IPasswordService>();
        _service = new AccountsService(
            _context,
            _mockPasswordService.Object,
            new Mock<IInventoryLogger<AccountsService>>().Object
        );
    }

    [Fact]
    public async Task GetAllAccountsAsync_WhenCalled_ReturnsAccountList()
    {
        // Arrange
        _context.Accounts.AddRange(
            new Account { Id = 1, Email = "user1@test.com", Name = "User 1" },
            new Account { Id = 2, Email = "user2@test.com", Name = "User 2" }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllAccountsAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAccountByIdAsync_ExistingId_ReturnsAccount()
    {
        // Arrange
        string email = "user1@test.com";
        _context.Accounts.AddRange(
            new Account { Id = 1, Email = email, Name = "User 1" },
            new Account { Id = 2, Email = "user2@test.com", Name = "User 2" }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAccountByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(email, result.Email);
    }

    [Fact]
    public async Task GetAccountByIdAsync_NonExistentId_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.GetAccountByIdAsync(999)
        );

        Assert.Equal("Account with ID 999 not found.", exception.Message);
    }

    [Fact]
    public async Task CreateAccountAsync_ValidAccount_ReturnsCreatedAccount()
    {
        // Arrange
        var account = new Account
        {
            Email = "user@t123.com",
            Name = "User",
        };
        string password = "123456";

        // Setup password service mock
        _mockPasswordService.Setup(x => x.Hash(password))
            .Returns("hashed_password_123");
        _mockPasswordService.Setup(x => x.VerifyPassword("hashed_password_123", password))
            .Returns(true);

        account.PasswordHash = _mockPasswordService.Object.Hash(password);
        // Act
        var result = await _service.CreateAccountAsync(account);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Name);
        Assert.Equal("user@t123.com", result.Email);
        Assert.True(_mockPasswordService.Object.VerifyPassword(result.PasswordHash, password));

        // Verify account was saved to database
        var savedAccount = await _context.Accounts.FindAsync(result.Id);
        Assert.NotNull(savedAccount);
    }

    [Fact]
    public async Task CreateAccountAsync_DuplicateEmail_ThrowsException()
    {
        // Arrange
        var existingAccounts = new List<Account>
        {
            new Account { Id = 1234,Email = "existing@test.com", PasswordHash = "123", Role = "User" }
        };

        _context.Accounts.AddRange(existingAccounts);
        _context.SaveChanges();

        var duplicateAccount = new Account
        {
            Id = 1235,
            Email = "existing@test.com",
            Role = "Admin",
            PasswordHash = "password"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAccountAsync(duplicateAccount)
        );
    }

    [Fact]
    public async Task AuthenticateAsync_ValidCredentials_ReturnsAccount()
    {
        // Arrange
        int index = 1234;
        var password = "correctPassword";
        var hashedPassword = "hashed_password";

        var accounts = new List<Account>
        {
            new Account
            {
                Id = index,
                PasswordHash = hashedPassword,
                Email = "user@test.com"
            }
        };

        _context.Accounts.AddRange(accounts);
        _context.SaveChanges();
        
        _mockPasswordService.Setup(p => p.VerifyPassword(hashedPassword, password))
                           .Returns(true);

        // Act
        var result = await _service.AuthenticateAsync(index, password);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(index, result.Id);
        Assert.Equal("user@test.com", result.Email);

    }

    [Fact]
public async Task AuthenticateAsync_InvalidPassword_ReturnsNull()
{
    // Arrange
    var index = 12345;
    var password = "wrongPassword";
    var hashedPassword = "hashed_password";
    
    _context.Accounts.AddRange(
        new Account 
        { 
            Id = index, 
            PasswordHash = hashedPassword,
            Email = "user@test.com"
        }
    );
    await _context.SaveChangesAsync();
    
    _mockPasswordService.Setup(p => p.VerifyPassword(hashedPassword, password))
                       .Returns(false);

    // Act
    var result = await _service.AuthenticateAsync(index, password);

    // Assert
    Assert.Null(result);
}

[Fact]
public async Task SetUserPassword_ValidData_UpdatesPassword()
{
    // Arrange
    _context.Accounts.AddRange(
        new Account 
        { 
            Id = 1, 
            PasswordHash = "old_password",
            Email = "user@test.com"
        }
    );
    await _context.SaveChangesAsync();

    var newPassword = "NewSecurePassword123";
    var hashedPassword = "new_hashed_password";

    _mockPasswordService.Setup(p => p.Hash(newPassword))
                       .Returns(hashedPassword);

    // Act
    await _service.resetPasswordOnNextLogin(1, true);
    await _service.SetUserPassword(1, newPassword);

    // Assert
    _mockPasswordService.Verify(p => p.Hash(newPassword), Times.Once);
    
    // Verify password was updated in database
    var updatedAccount = await _context.Accounts.FindAsync(1);
    Assert.Equal(hashedPassword, updatedAccount.PasswordHash);
}

[Fact]
public async Task UpdateAccountAsync_ValidData_ReturnsUpdatedAccount()
{
    // Arrange
    _context.Accounts.AddRange(
        new Account 
        { 
            Id = 1, 
            Email = "old@test.com", 
            Name = "Old Name",
            PasswordHash = "hash"
        }
    );
    await _context.SaveChangesAsync();

    var updatedAccount = new Account
    {
        Id = 1,
        Email = "updated@test.com",
        Name = "Updated Name",
        PasswordHash = "hash"
    };

    // Act
    var result = await _service.UpdateAccountAsync(updatedAccount);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("updated@test.com", result.Email);
    Assert.Equal("Updated Name", result.Name);
    
    // Verify changes were saved to database
    var savedAccount = await _context.Accounts.FindAsync(1);
    Assert.Equal("updated@test.com", savedAccount.Email);
    Assert.Equal("Updated Name", savedAccount.Name);
}

[Fact]
public async Task DeleteAccountAsync_ExistingId_DeletesAccount()
{
    // Arrange
    _context.Accounts.AddRange(
        new Account 
        { 
            Id = 2, 
            Email = "delete@test.com",
            PasswordHash = "hash"
        }
    );
    await _context.SaveChangesAsync();

    // Act
    await _service.DeleteAccountAsync(2);

    // Assert
    var deletedAccount = await _context.Accounts.FindAsync(1);
    Assert.Null(deletedAccount);
}
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

