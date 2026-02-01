// using System;

// namespace Tests;

// public class StocktakeServiceTests
//     {
//         private readonly Mock<MyDbContext> _mockContext;
//         private readonly Mock<IInventoryService> _mockInventoryService;
//         private readonly StocktakeService _service;

//         public StocktakeServiceTests()
//         {
//             _mockContext = new Mock<MyDbContext>();
//             _mockInventoryService = new Mock<IInventoryService>();
//             _service = new StocktakeService(_mockContext.Object, _mockInventoryService.Object);
//         }

//         [Fact]
//         public async Task CreateNewStocktake_ValidData_CreatesStocktake()
//         {
//             // Arrange
//             var items = new List<InventoryItem>
//             {
//                 new InventoryItem { Id = 1, ItemName = "Item 1" },
//                 new InventoryItem { Id = 2, ItemName = "Item 2" }
//             };
//             var accounts = new List<Account>
//             {
//                 new Account { Id = 1, Email = "user1@test.com" }
//             };

//             _mockContext.Setup(x => x.Stocktakes).ReturnsDbSet(new List<Stocktake>());
//             _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
//                        .ReturnsAsync(1);

//             // Act
//             await _service.CreateNewStocktake(
//                 "Q4 2024 Stocktake", 
//                 "End of year inventory check", 
//                 items, 
//                 accounts, 
//                 DateTime.Now, 
//                 DateTime.Now.AddDays(7)
//             );

//             // Assert
//             _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
//         }

//         [Fact]
//         public async Task CreateNewStocktake_EmptyItemsList_ThrowsValidationException()
//         {
//             // Arrange
//             var emptyItems = new List<InventoryItem>();
//             var accounts = new List<Account> { new Account { Id = 1 } };

//             // Act & Assert
//             await Assert.ThrowsAsync<ArgumentException>(
//                 () => _service.CreateNewStocktake(
//                     "Test", "Desc", emptyItems, accounts, 
//                     DateTime.Now, DateTime.Now.AddDays(1))
//             );
//         }

//         [Fact]
//         public async Task GetAllStocktakesAsync_ReturnsAllStocktakes()
//         {
//             // Arrange
//             var stocktakes = new List<Stocktake>
//             {
//                 new Stocktake { Id = 1, Name = "Stocktake 1" },
//                 new Stocktake { Id = 2, Name = "Stocktake 2" }
//             };
            
//             _mockContext.Setup(x => x.Stocktakes).ReturnsDbSet(stocktakes);

//             // Act
//             var result = await _service.GetAllStocktakesAsync();

//             // Assert
//             result.Should().HaveCount(2);
//             result.Should().Contain(s => s.Name == "Stocktake 1");
//         }

//         [Fact]
//         public async Task GetStocktakeById_ExistingId_ReturnsStocktake()
//         {
//             // Arrange
//             var stocktakes = new List<Stocktake>
//             {
//                 new Stocktake
//                 {
//                     Id = 1,
//                     Name = "Test Stocktake",
//                     Items = new List<InventoryItem>()
//                 }
//             };
            
//             _mockContext.Setup(x => x.Stocktakes).ReturnsDbSet(stocktakes);

//             // Act
//             var result = await _service.GetStocktakeById(1);

//             // Assert
//             result.Should().NotBeNull();
//             result.Name.Should().Be("Test Stocktake");
//         }

//         [Fact]
//         public async Task MarkItemAsChecked_ValidData_MarksItemSuccessfully()
//         {
//             // Arrange
//             var item = new InventoryItem { Id = 5, ItemName = "Test Item" };
//             var stocktakes = new List<Stocktake>
//             {
//                 new Stocktake
//                 {
//                     Id = 1,
//                     Name = "Test",
//                     Items = new List<InventoryItem> { item },
//                     CheckedItems = new List<StocktakeCheckedItem>()
//                 }
//             };
            
//             _mockContext.Setup(x => x.Stocktakes).ReturnsDbSet(stocktakes);
//             _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
//                        .ReturnsAsync(1);

//             // Act
//             await _service.MarkItemAsChecked(1, 5, "user@test.com");

//             // Assert
//             _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
//         }

//         [Fact]
//         public async Task GetStocktakeStatistics_CalculatesCorrectly()
//         {
//             // Arrange
//             var items = new List<InventoryItem>
//             {
//                 new InventoryItem { Id = 1, ItemName = "Item 1" },
//                 new InventoryItem { Id = 2, ItemName = "Item 2" },
//                 new InventoryItem { Id = 3, ItemName = "Item 3" },
//                 new InventoryItem { Id = 4, ItemName = "Item 4" }
//             };
            
//             var stocktakes = new List<Stocktake>
//             {
//                 new Stocktake
//                 {
//                     Id = 1,
//                     Items = items,
//                     CheckedItems = new List<StocktakeCheckedItem>
//                     {
//                         new StocktakeCheckedItem { ItemId = 1 },
//                         new StocktakeCheckedItem { ItemId = 2 }
//                     }
//                 }
//             };

//             _mockContext.Setup(x => x.Stocktakes).ReturnsDbSet(stocktakes);

//             // Act
//             var result = await _service.GetStocktakeStatistics(1);

//             // Assert
//             result.TotalItems.Should().Be(4);
//             result.CheckedItems.Should().Be(2);
//             result.UncheckedItems.Should().Be(2);
//             result.CompletionPercentage.Should().Be(50);
//         }

//         [Fact]
//         public async Task IsUserAuthorized_AuthorizedUser_ReturnsTrue()
//         {
//             // Arrange
//             var account = new Account { Id = 5, Email = "auth@test.com" };
//             var stocktakes = new List<Stocktake>
//             {
//                 new Stocktake
//                 {
//                     Id = 1,
//                     AuthorizedAccounts = new List<Account> { account }
//                 }
//             };

//             _mockContext.Setup(x => x.Stocktakes).ReturnsDbSet(stocktakes);

//             // Act
//             var result = await _service.IsUserAuthorized(1, 5);

//             // Assert
//             result.Should().BeTrue();
//         }

//         [Fact]
//         public async Task DeleteStocktakeAsync_ExistingStocktake_DeletesSuccessfully()
//         {
//             // Arrange
//             var stocktakes = new List<Stocktake>
//             {
//                 new Stocktake { Id = 1, Name = "Delete Me" }
//             };
            
//             _mockContext.Setup(x => x.Stocktakes).ReturnsDbSet(stocktakes);
//             _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
//                        .ReturnsAsync(1);

//             // Act
//             var result = await _service.DeleteStocktakeAsync(1);

//             // Assert
//             result.Should().NotBeNull();
//             _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
//         }
//     }