using InventoryLibrary.Data;
using InventoryLibrary.Model.Inventory;
using InventoryLibrary.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace InventoryLibrary.Services
{
    public class InventoryService : IInventoryService
    {

        private readonly MyDbContext _context;

        public InventoryService(MyDbContext context)
        {
            _context = context;
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
                // Log the exception (you can use any logging framework)
                Console.WriteLine($"An error occurred while retrieving items: {ex.Message}");
                return null;
            }
        }

        public async Task<InventoryItem> GetItemByIdAsync(int id)
        {
            var item = await _context.InventoryItems.FindAsync(id);

            return item;
        }

        public async Task<InventoryItem> CreateItemAsync(InventoryItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            item.addedDate = DateTime.UtcNow;
            item.lastInventoryDate = DateTime.UtcNow;
            var barcodeGen = new BarcodeGenerator();
            var barcodeNumber = await barcodeGen.GenerateBarcodeNumber(_context);
            item.Barcode = barcodeNumber;
            _context.InventoryItems.Add(item);
            await _context.SaveChangesAsync();
            return item;
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

            _context.InventoryItems.Remove(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<InventoryItem> UpdateItemAsync(InventoryItem item)
        {
            var existingItem = await _context.InventoryItems.FindAsync(item.Id);
            if (existingItem == null)
            {
                throw new KeyNotFoundException($"Item with ID {item.Id} not found.");
            }

            // Update properties
            existingItem = item;
            _context.InventoryItems.Update(existingItem);
            await _context.SaveChangesAsync();
            return existingItem;
        }
    }
}
