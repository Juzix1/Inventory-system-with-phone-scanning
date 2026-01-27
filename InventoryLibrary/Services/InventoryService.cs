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
                _logger?.LogError("Error in GetAllItemsAsync",ex);
                throw;
            }
        }

        public async Task<InventoryItem> GetItemByIdAsync(int id)
        {
            try
            {
                var item = await _context.InventoryItems.FindAsync(id);
                return item;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error in GetItemByIdAsync for id {id}",ex);
                throw;
            }
        }

        public async Task<InventoryItem> CreateItemAsync(InventoryItem item)
        {
            _logger.LogInfo("Creating new Inventory Item");
            try
            {
                if (item == null)
                {
                    throw new ArgumentNullException(nameof(item));
                }
                item.addedDate = DateTime.Now;
                item.lastInventoryDate = DateTime.Now;
                _context.InventoryItems.Add(item);

                await _context.SaveChangesAsync();
                return item;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error creating item: {item}",ex);
                throw;
            }
        }

        public async Task<IEnumerable<InventoryItem>> GetItemsByName(string name)
        {
            _logger.LogInfo("Getting items by Name");
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
            _logger.LogWarning($"Removing item with id {id} from Inventory database, and saving to Historical database");
            try
            {
                var item = await _context.InventoryItems.FindAsync(id);
                if (item == null)
                {
                    _logger.LogError($"Error: Can't remove item with id {id}, doesn't exist!");
                    throw new KeyNotFoundException($"Item with ID {id} not found.");
                }
                await _historicalDataService.CreateHistoricalItemAsync(item);
                _context.InventoryItems.Remove(item);
                await _context.SaveChangesAsync();
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
            _logger.LogInfo($"Updating item with id {item.Id}");
            try
            {
                var existingItem = await _context.InventoryItems.FindAsync(item.Id);
                if (existingItem == null)
                {
                    _logger.LogError($"Error: Can't update item with id {item.Id}, doesn't exist!");
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



                // Update properties safely on the tracked entity
                _context.Entry(existingItem).CurrentValues.SetValues(item);
                await _context.SaveChangesAsync();
                return existingItem;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error updating item: {item}", ex);
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
                _logger?.LogError($"Error in UpdateItemInventory for item {item.Id}", ex);
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
