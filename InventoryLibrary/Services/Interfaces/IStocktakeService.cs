using System;
using InventoryLibrary.Model.Inventory;
using InventoryLibrary.Model.StockTake;

namespace InventoryLibrary.Services.Interfaces;

public interface IStocktakeService
{
    Task CreateNewStocktake(List<InventoryItem> items, DateTime? startDate, DateTime endDate);
    Task MarkItem(int id,InventoryItem item);
    Task<Stocktake> UpdateStocktake(Stocktake stocktake);
    Task<IEnumerable<Stocktake>> GetAllStocktakesAsync();
    Task<Stocktake> GetStocktakeById(int id);
}
