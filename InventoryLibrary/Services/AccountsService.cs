using InventoryLibrary.Data;
using InventoryLibrary.Model.Account;
using InventoryLibrary.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryLibrary.Services
{
    public class AccountsService : IAccountsService
    {
        private readonly MyDbContext _context;

        public AccountsService(MyDbContext context)
        {
            _context = context;
        }

        public async Task<Account> AuthenticateAsync(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Email and password cannot be null or empty.");
            }

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email && a.PasswordHash == password);


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
            }

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
        }

        public async Task<Account> GetAccountByIdAsync(int id)
        {
            var account = await _context.Accounts.FindAsync(id);

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
                throw new KeyNotFoundException("No accounts found.");
            }
            return accounts;
        }

        public async Task<Account> UpdateAccountAsync(int id, Account account)
        {
            var existingAccount = await _context.Accounts.FindAsync(id);
            if (existingAccount == null)
            {
                throw new KeyNotFoundException($"Account with ID {id} not found.");
            }

            if (!await IsEmailUniqueAsync(account.Email))
            {
                throw new ArgumentException("Email must be unique and valid.");
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
    }
}
