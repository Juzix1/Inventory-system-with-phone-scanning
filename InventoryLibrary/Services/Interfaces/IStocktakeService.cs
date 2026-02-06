using InventoryLibrary.Model.Accounts;
using InventoryLibrary.Model.DTO.Stocktake;
using InventoryLibrary.Model.Inventory;
using InventoryLibrary.Model.StockTake;

namespace InventoryLibrary.Services.Interfaces
{
    public interface IStocktakeService
    {
        Task CreateNewStocktake(
            string name,
            string description,
            List<InventoryItem> items,
            List<Account> authorizedAccounts,
            DateTime startDate,
            DateTime endDate);
        Task CreateNewStocktake(
            List<InventoryItem> items,
            DateTime? startDate,
            DateTime endDate);
        Task<IEnumerable<Stocktake>> GetAllStocktakesAsync();
        Task<IEnumerable<Stocktake>> GetStocktakesByAccount(int id);
        Task<Stocktake> GetStocktakeById(int id);
        Task<Stocktake> UpdateStocktake(Stocktake stocktake);
        Task MarkItemAsChecked(int stocktakeId, int itemId, string checkedBy = null);
        Task MarkItem(int id, InventoryItem item);
        Task UnmarkItemAsChecked(int stocktakeId, int itemId);
        Task<Stocktake> DeleteStocktakeAsync(int id);
        Task<StocktakeStatistics> GetStocktakeStatistics(int stocktakeId);
        Task<List<InventoryItem>> GetUncheckedItems(int stocktakeId);
        Task<bool> IsUserAuthorized(int stocktakeId, int userId);
        Task<List<StocktakeCheckedItem>> GetCheckedItemsDetails(int stocktakeId);
        Task<bool> IsItemChecked(int stocktakeId, int itemId);
        Task MarkMultipleItemsAsChecked(int stocktakeId, List<int> itemIds, string checkedBy = null);
        Task UnmarkMultipleItemsAsChecked(int stocktakeId, List<int> itemIds);
        Task<int> GetCheckedItemsCount(int stocktakeId);
        Task<int> GetProgressPercentage(int stocktakeId);
        Task<List<int>> GetAssignedItemsIds(int stocktakeId);
    }
}