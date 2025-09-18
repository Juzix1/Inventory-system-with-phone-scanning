using InventoryLibrary.Model.Inventory;

namespace InventoryLibrary.Services.Interfaces
{
    public interface IInventoryService
    {
        Task<IEnumerable<InventoryItem>> GetAllItemsAsync();
        Task<InventoryItem> GetItemByIdAsync(int id);
    }

}
