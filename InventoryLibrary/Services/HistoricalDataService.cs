using System;
using InventoryLibrary.Data;
using InventoryLibrary.Model.Inventory;
using InventoryLibrary.Services.Interfaces;

namespace InventoryLibrary.Services;

public class HistoricalDataService : IHistoricalDataService
{
    private readonly MyDbContext _context;
    private readonly IInventoryLogger<HistoricalDataService> _logger;
    public HistoricalDataService(MyDbContext context, IInventoryLogger<HistoricalDataService> logger)
    {
        _context = context;
        _logger = logger;
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
            _logger.LogInfo($"Created historical item for InventoryItem ID: {item.Id}");
            return Task.FromResult(historicalItem);
        }catch (Exception ex)
        {
            _logger.LogError("Error in creating historical item", ex);
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
                _logger.LogWarning($"Historical item with ID {id} not found for deletion.");
                throw new KeyNotFoundException($"Historical item with ID {id} not found.");
            }

            _context.HistoricalItems.Remove(historicalItem);
            _context.SaveChanges();
            _logger.LogWarning($"Deleted historical item with ID: {id}");
            return Task.CompletedTask;
        }catch (Exception ex)
        {
            _logger.LogError("Error in deleting historical item", ex);
            throw;
        }
    }

    public Task<IEnumerable<HistoricalItem>> GetAllHistoricalItemsAsync()
    {
        try{
            var historicalItems = _context.HistoricalItems.ToList();
            return Task.FromResult<IEnumerable<HistoricalItem>>(historicalItems);
        }catch (Exception ex)
        {
            _logger.LogError("Error in retrieving all historical items", ex);
            throw;
        }
    }

    public Task<HistoricalItem> GetHistoricalItemByIdAsync(int id)
    {
        try
        {
            var historicalItem = _context.HistoricalItems.Find(id);
            if (historicalItem == null)
            {
                _logger.LogWarning($"Historical item with ID {id} not found.");
                throw new KeyNotFoundException($"Historical item with ID {id} not found.");
            }
            return Task.FromResult(historicalItem);
        }catch (Exception ex)
        {
            _logger.LogError("Error in retrieving historical item by ID", ex);
            throw;
        }
    }

    public Task<HistoricalItem> UpdateHistoricalItemAsync(InventoryItem item)
    {
        try
        {
            var existingHistoricalItem = _context.HistoricalItems.FirstOrDefault(h => h.OriginalItemId == item.Id);
            if (existingHistoricalItem == null)
            {
                _logger.LogWarning($"Historical item for original item ID {item.Id} not found for update.");
                throw new KeyNotFoundException($"Historical item for original item ID {item.Id} not found.");
            }

            existingHistoricalItem.itemName = item.itemName;
            existingHistoricalItem.itemDescription = item.itemDescription;
            existingHistoricalItem.ItemTypeId = item.ItemTypeId;
            existingHistoricalItem.itemWeight = item.itemWeight;
            existingHistoricalItem.itemPrice = item.itemPrice;

            _context.HistoricalItems.Update(existingHistoricalItem);
            _context.SaveChanges();
            _logger.LogInfo($"Updated historical item for OriginalItemId: {item.Id}");
            return Task.FromResult(existingHistoricalItem);
        }catch (Exception ex)
        {
            _logger.LogError("Error in updating historical item", ex);
            throw;
        }
    }
}
