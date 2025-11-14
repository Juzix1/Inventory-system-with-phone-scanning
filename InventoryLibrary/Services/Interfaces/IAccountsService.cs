using InventoryLibrary.Model.Accounts;

namespace InventoryLibrary.Services.Interfaces
{
    public interface IAccountsService
    {
        Task<IEnumerable<Account>> GetAllAccountsAsync();
        Task<Account> GetAccountByIdAsync(int id);
        Task<Account> CreateAccountAsync(Account account);
        Task<Account> UpdateAccountAsync(Account account);
        Task DeleteAccountAsync(int id);
        Task<Account> AuthenticateAsync(string email, string password);
        Task<bool> CanAccessScanner(int accountId);
    }
}
