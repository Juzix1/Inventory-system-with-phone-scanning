using InventoryLibrary.Model.Inventory;

namespace InventoryLibrary.Services.Interfaces
{
    public interface IInventoryService
    {
        Task<IEnumerable<InventoryItem>> GetAllItemsAsync();
        Task<InventoryItem> GetItemByIdAsync(int id);
        Task<InventoryItem> CreateItemAsync(InventoryItem item);
        Task<IEnumerable<InventoryItem>> GetItemsByName(string name);
        Task<InventoryItem> UpdateItemAsync(InventoryItem item);
    }

}
