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
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(MyDbContext context, IHistoricalDataService historic,ILogger<InventoryService> logger)
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
                _logger?.LogError(ex, "Error in GetAllItemsAsync");
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
                _logger?.LogError(ex, "Error in GetItemByIdAsync for id {Id}", id);
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
                item.addedDate = DateTime.Now;
                item.lastInventoryDate = DateTime.Now;
                var barcodeGen = new BarcodeGenerator();
                var barcodeNumber = await barcodeGen.GenerateBarcodeNumber(_context);
                item.Barcode = barcodeNumber;
                _context.InventoryItems.Add(item);
                
                await _context.SaveChangesAsync();
                return item;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating item: {@Item}", item);
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
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<InventoryItem>();
            }
        }

        public async Task<InventoryItem> DeleteItemAsync(int id)
        {
            var item = await _context.InventoryItems.FindAsync(id);
            if (item == null)
            {
                throw new KeyNotFoundException($"Item with ID {id} not found.");
            }
            await _historicalDataService.CreateHistoricalItemAsync(item);
            _context.InventoryItems.Remove(item);
            await _context.SaveChangesAsync();
            return item;
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
                if(existingItem.personInCharge?.Id != null || existingItem.PersonInChargeId != null)
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
                _logger?.LogError(ex, "Error updating item: {@Item}", item);
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
                _logger?.LogError(ex, "Error in UpdateItemInventory for item {ItemId}", item?.Id);
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
            _logger?.LogError(ex, "Error deleting image reference for item {ItemId}", inventoryItemId);
            throw;
        }
    }


    }
}
