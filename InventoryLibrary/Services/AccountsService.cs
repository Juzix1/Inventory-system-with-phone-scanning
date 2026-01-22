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
        

        public AccountsService(MyDbContext context, IPasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        public async Task<Account?> AuthenticateAsync(int index, string password)
        {
            if (index == 0 || string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Email and password cannot be null or empty.");
            }

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == index);
            if(account == null){
                return null;
            }
            if(!_passwordService.VerifyPassword(account.PasswordHash, password)){
                if(account.resetPasswordOnNextLogin && account.PasswordHash == ""){
                    return account;
                }
                return null;
            }

            return account;
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
                return account;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Error creating account.", ex);
            }
        }

        public async Task DeleteAccountAsync(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                throw new KeyNotFoundException($"Account with ID {id} not found.");
            }else if(account.Id == 1)
            {
                throw new InvalidOperationException("Cannot delete the admin account.");
            }
            else
            {
                _context.Accounts.Remove(account);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Account> GetAccountByIdAsync(int id)
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

        public async Task<IEnumerable<Account>> GetAllAccountsAsync()
        {
            var accounts = await _context.Accounts.ToListAsync();
            if (accounts == null || !accounts.Any())
            {
                return Enumerable.Empty<Account>();
            }
            return accounts;
        }

        public async Task<Account> UpdateAccountAsync(Account account)
        {
            var existingAccount = await _context.Accounts.FindAsync(account.Id);
            if (existingAccount == null)
            {
                throw new KeyNotFoundException($"Account with ID {account.Id} not found.");
            }
            
            if(existingAccount.Email != account.Email){
                if (!await IsEmailUniqueAsync(account.Email))
                {
                    throw new ArgumentException("Email must be unique and valid.");
                }
            }
            if(existingAccount.Role != account.Role && existingAccount.Id == 1)
            {
                throw new InvalidOperationException("Cannot change the role of the first admin account.");
            }
            _context.Entry(existingAccount).CurrentValues.SetValues(account);
            await _context.SaveChangesAsync();
            return account;
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

        // do wywalenia bo i tak inaczej to będzie się sprawdzało 
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
            var account = _context.Accounts.Find(accountId);
            if (account == null)
            {
                throw new KeyNotFoundException($"Account with ID {accountId} not found.");
            }
            account.PasswordHash = _passwordService.Hash(newPassword);
            account.resetPasswordOnNextLogin = false;
            _context.SaveChanges();
            return Task.CompletedTask;
        }

        public Task resetPasswordOnNextLogin(int accountId, bool reset)
        {
             var account = _context.Accounts.Find(accountId);
            if (account == null)
            {
                throw new KeyNotFoundException($"Account with ID {accountId} not found.");
            }
            account.resetPasswordOnNextLogin = true;
            _context.SaveChanges();
            return Task.CompletedTask;
        }
    }
}
