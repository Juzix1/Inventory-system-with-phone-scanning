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
        Task<Account?> AuthenticateAsync(int index, string password);
        Task SetUserPassword(int accountId, string newPassword);
        Task resetPasswordOnNextLogin(int accountId, bool reset);
    }
}
