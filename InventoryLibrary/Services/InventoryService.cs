using InventoryLibrary.Data;
using InventoryLibrary.Model.Inventory;
using InventoryLibrary.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;

namespace InventoryLibrary.Services
{
    public class InventoryService : IInventoryService
    {

        private readonly MyDbContext _context;
        private readonly IHistoricalDataService _historicalDataService;
        private readonly IInventoryLogger<InventoryService> _logger;

        public InventoryService(MyDbContext context, IHistoricalDataService historic, IInventoryLogger<InventoryService> logger)
        {
            _context = context;
            _logger = logger;
            _historicalDataService = historic;
        }

        public async Task<IEnumerable<InventoryItem>> GetAllItemsAsync()
        {
            try
            {
                var items = await _context.InventoryItems
                    .Include(i => i.Location)
                        .ThenInclude(r => r.Department)
                    .Include(i => i.ItemType)
                    .Include(i => i.ItemCondition)
                    .ToListAsync();
                return items;
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error while getting all items info",ex);
                throw;
            }
        }
        public async Task<InventoryItem> GetItemByIdAsync(int id)
        {
            try
            {
                var item = await _context.InventoryItems.FindAsync(id);
                if(item == null)
                {
                    throw new KeyNotFoundException("The item with this id doesnt exist");
                }
                return item;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error while getting item with id {id}",ex);
                throw;
            }
        }

        public async Task<InventoryItem> CreateItemAsync(InventoryItem item)
        {
            try
            {
                if (item == null)
                {
                    throw new ArgumentNullException(nameof(item));
                }
                if (item.itemName == null || item.itemName.IsNullOrEmpty())
                {
                    throw new ArgumentException("Item name can't be empty");
                }
                item.addedDate = DateTime.Now;
                item.lastInventoryDate = DateTime.Now;
                _context.InventoryItems.Add(item);

                await _context.SaveChangesAsync();
                _logger?.LogInfo($"Created new item: {item}");
                return item;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error while creating item: {item}",ex);
                throw;
            }
        }

        public async Task<IEnumerable<InventoryItem>> GetItemsByName(string name)
        {
            try
            {
                var items = await _context.InventoryItems.Where(i => i.itemName == name).ToListAsync();

                if (items.IsNullOrEmpty())
                {
                    throw new ArgumentNullException();
                }
                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting item with name {name}", ex);
                return new List<InventoryItem>();
            }
        }

        public async Task<InventoryItem> DeleteItemAsync(int id)
        {
            try
            {
                var item = await _context.InventoryItems.FindAsync(id);
                if (item == null)
                {
                    throw new KeyNotFoundException($"Item with ID {id} not found.");
                }
                await _historicalDataService.CreateHistoricalItemAsync(item);
                _context.InventoryItems.Remove(item);
                await _context.SaveChangesAsync();
                _logger.LogWarning($"Deleted item with id {id}");
                return item;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error deleting item with id {id}", ex);
                throw;
            }
        }

        public async Task<InventoryItem> UpdateItemAsync(InventoryItem item)
        {
            try
            {
                var existingItem = await _context.InventoryItems.FindAsync(item.Id);
                if (existingItem == null)
                {
                    throw new KeyNotFoundException($"Item with ID {item.Id} not found.");
                }
                if (existingItem.personInCharge?.Id != null || existingItem.PersonInChargeId != null)
                {
                    existingItem.Location = null;
                    existingItem.RoomId = null;
                }
                else
                {
                    existingItem.PersonInChargeId = null;
                    existingItem.personInCharge = null;
                }
                _context.Entry(existingItem).CurrentValues.SetValues(item);
                await _context.SaveChangesAsync();
                _logger?.LogInfo($"Updated item: {item}");
                return existingItem;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error while updating item: {item}", ex);
                throw;
            }
        }

        public async Task UpdateItemInventory(InventoryItem item)
        {
            try
            {
                var existingItem = await _context.InventoryItems.FindAsync(item.Id) ?? throw new KeyNotFoundException($"Item with ID {item.Id} not found.");
                if (existingItem.personInCharge?.Id != null || existingItem.PersonInChargeId != null)
                {
                    existingItem.Location = null;
                    existingItem.RoomId = null;
                }
                else
                {
                    existingItem.PersonInChargeId = null;
                    existingItem.personInCharge = null;
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error while updating item {item.Id}", ex);
                throw;
            }
        }

        public async Task DeleteImageAsync(int inventoryItemId)
        {
            try
            {
                var inventoryItem = await _context.InventoryItems.FindAsync(inventoryItemId);
                if (inventoryItem == null)
                    throw new InvalidOperationException("Inventory item not found");

                if (!string.IsNullOrEmpty(inventoryItem.imagePath))
                {
                    inventoryItem.imagePath = null;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error deleting image reference for item {inventoryItemId}", ex);
                throw;
            }
        }


    }
}
