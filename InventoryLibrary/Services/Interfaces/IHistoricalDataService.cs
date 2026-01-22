using System;
using InventoryLibrary.Model.Inventory;

namespace InventoryLibrary.Services.Interfaces;

public interface IHistoricalDataService
{
    Task<IEnumerable<HistoricalItem>> GetAllHistoricalItemsAsync();
    Task<HistoricalItem> GetHistoricalItemByIdAsync(int id);
    Task<HistoricalItem> CreateHistoricalItemAsync(InventoryItem item);
    Task<HistoricalItem> UpdateHistoricalItemAsync(InventoryItem item);
    Task DeleteHistoricalItemAsync(int id);

}
