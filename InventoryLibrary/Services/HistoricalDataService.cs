using System;
using InventoryLibrary.Data;
using InventoryLibrary.Model.Inventory;
using InventoryLibrary.Services.Interfaces;

namespace InventoryLibrary.Services;

public class HistoricalDataService : IHistoricalDataService
{
    private readonly MyDbContext _context;
    public HistoricalDataService(MyDbContext context)
    {
        _context = context;
        
    }

    public Task<HistoricalItem> CreateHistoricalItemAsync(InventoryItem item)
    {
        try
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            var historicalItem = new HistoricalItem
            {
                OriginalItemId = item.Id,
                itemName =  item.itemName,
                itemDescription = item.itemDescription,
                ItemTypeId = item.ItemTypeId,
                itemWeight = item.itemWeight,
                itemPrice = item.itemPrice,

                addedDate = item.addedDate,
                archivedDate = DateTime.Now,
            };

            _context.HistoricalItems.Add(historicalItem);
            _context.SaveChanges();
            return Task.FromResult(historicalItem);
        }catch (Exception ex)
        {
            throw;
        }
    }

    public Task DeleteHistoricalItemAsync(int id)
    {
        try
        {
            var historicalItem = _context.HistoricalItems.Find(id);
            if (historicalItem == null)
            {
                throw new KeyNotFoundException($"Historical item with ID {id} not found.");
            }

            _context.HistoricalItems.Remove(historicalItem);
            _context.SaveChanges();
            return Task.CompletedTask;
        }catch (Exception ex)
        {
            throw;
        }
    }

    public Task<IEnumerable<HistoricalItem>> GetAllHistoricalItemsAsync()
    {
        var historicalItems = _context.HistoricalItems.ToList();
        return Task.FromResult((IEnumerable<HistoricalItem>)historicalItems);
    }

    public Task<HistoricalItem> GetHistoricalItemByIdAsync(int id)
    {
        var historicalItem = _context.HistoricalItems.Find(id);
        if (historicalItem == null)
        {
            throw new KeyNotFoundException($"Historical item with ID {id} not found.");
        }
        return Task.FromResult(historicalItem);
    }

    public Task<HistoricalItem> UpdateHistoricalItemAsync(InventoryItem item)
    {
        try
        {
            var existingHistoricalItem = _context.HistoricalItems.FirstOrDefault(h => h.OriginalItemId == item.Id);
            if (existingHistoricalItem == null)
            {
                throw new KeyNotFoundException($"Historical item for original item ID {item.Id} not found.");
            }

            existingHistoricalItem.itemName = item.itemName;
            existingHistoricalItem.itemDescription = item.itemDescription;
            existingHistoricalItem.ItemTypeId = item.ItemTypeId;
            existingHistoricalItem.itemWeight = item.itemWeight;
            existingHistoricalItem.itemPrice = item.itemPrice;

            _context.HistoricalItems.Update(existingHistoricalItem);
            _context.SaveChanges();
            return Task.FromResult(existingHistoricalItem);
        }catch (Exception ex)
        {
            throw;
        }
    }
}
