using InventoryLibrary.Data;
using InventoryLibrary.Model.Accounts;
using InventoryLibrary.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryLibrary.Services
{
    public class AccountsService : IAccountsService
    {
        private readonly MyDbContext _context;
        private readonly IPasswordService _passwordService;
        private readonly IInventoryLogger<AccountsService> _logger;


        public AccountsService(MyDbContext context, IPasswordService passwordService, IInventoryLogger<AccountsService> logger)
        {
            _context = context;
            _passwordService = passwordService;
            _logger = logger;
        }

        public async Task<Account?> AuthenticateAsync(int index, string password)
        {
            try
            {
                if (index == 0 || string.IsNullOrEmpty(password))
                {
                    throw new ArgumentException("Email and password cannot be null or empty.");
                }

                var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == index);
                if (account == null)
                {
                    return null;
                }
                if (!_passwordService.VerifyPassword(account.PasswordHash, password))
                {
                    if (account.resetPasswordOnNextLogin && account.PasswordHash == "")
                    {
                        return account;
                    }
                    return null;
                }
                _logger.LogInfo($"User authenticated {(account.IsAdmin ? "with access" : "without access")} with id {account.Id}");
                return account;
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error while authenticating user {ex.Message}");
                return null;
            }
        }

        public async Task<Account> CreateAccountAsync(Account account)
        {
            if (!await IsEmailUniqueAsync(account.Email))
            {
                throw new ArgumentException("Email must be unique and valid.");
            }

            _context.Accounts.Add(account);
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInfo($"Created new account with id {account.Id}");
                return account;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError("Error while creating account", ex);
                throw new InvalidOperationException("Error creating account.", ex);
            }
        }

        public async Task DeleteAccountAsync(int id)
        {
            try
            {
                var account = await _context.Accounts.FindAsync(id);
                if (account == null)
                {
                    throw new KeyNotFoundException($"Account with ID {id} not found.");
                }
                else if (account.Id == 1)
                {
                    throw new InvalidOperationException("Cannot delete the admin account.");
                }
                else
                {
                    _context.Accounts.Remove(account);
                    await _context.SaveChangesAsync();
                    _logger.LogWarning($"Deleted account with id {id}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while deleting account", ex);
                throw new Exception("Error deleting account", ex);
            }
        }

        public async Task<Account> GetAccountByIdAsync(int id)
        {
            try
            {
                var account = await _context.Accounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

                if (account == null)
                {
                    throw new KeyNotFoundException($"Account with ID {id} not found.");
                }

                return account;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while retrieving account with id {id}", ex);
                throw;
            }
        }

        public async Task<IEnumerable<Account>> GetAllAccountsAsync()
        {
            try
            {
                var accounts = await _context.Accounts.ToListAsync();
                if (accounts == null || !accounts.Any())
                {
                    return Enumerable.Empty<Account>();
                }
                return accounts;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while retrieving all accounts", ex);
                throw;
            }
        }

        public async Task<Account> UpdateAccountAsync(Account account)
        {
            try
            {
                var existingAccount = await _context.Accounts.FindAsync(account.Id);
                if (existingAccount == null)
                {
                    throw new KeyNotFoundException($"Account with ID {account.Id} not found.");
                }

                if (existingAccount.Email != account.Email)
                {
                    if (!await IsEmailUniqueAsync(account.Email))
                    {
                        throw new ArgumentException("Email must be unique and valid.");
                    }
                }
                if (existingAccount.Role != account.Role && existingAccount.Id == 1)
                {
                    throw new InvalidOperationException("Cannot change the role of the first admin account.");
                }
                _context.Entry(existingAccount).CurrentValues.SetValues(account);
                await _context.SaveChangesAsync();
                _logger.LogInfo($"Updated account with id {account.Id}");
                return account;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while updating account with id {account.Id}", ex);
                throw;
            }
        }

        private async Task<bool> IsEmailUniqueAsync(string email)
        {
            if (!email.Contains("@") || !email.Contains("."))
            {
                return false;
            }

            var existingAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email);
            return existingAccount == null;
        }

        public async Task<bool> CanAccessScanner(int accountId)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null)
            {
                throw new KeyNotFoundException($"Account with ID {accountId} not found.");
            }
            if (account.IsAdmin || account.IsModerator)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public Task SetUserPassword(int accountId, string newPassword)
        {
            try
            {
                var account = _context.Accounts.Find(accountId);
                if (account == null)
                {
                    throw new KeyNotFoundException($"Account with ID {accountId} not found.");
                }
                else if (account.resetPasswordOnNextLogin == true)
                {
                    account.PasswordHash = _passwordService.Hash(newPassword);
                    account.resetPasswordOnNextLogin = false;
                    _context.SaveChanges();
                }
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while setting password for account id {accountId}", ex);
                throw;
            }
        }

        public Task resetPasswordOnNextLogin(int accountId, bool reset)
        {
            try
            {
                var account = _context.Accounts.Find(accountId);
            if (account == null)
            {
                throw new KeyNotFoundException($"Account with ID {accountId} not found.");
            }
            account.resetPasswordOnNextLogin = true;
            _context.SaveChanges();
            _logger.LogWarning($"Password set for reset on next login for account id {accountId}");
            return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while resetting password for account id {accountId}", ex);
                throw;
            }
        }
    }
}
