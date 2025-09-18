using InventoryLibrary.Data;
using InventoryLibrary.Model.Inventory;
using InventoryLibrary.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryLibrary.Services
{
    public class InventoryService : IInventoryService
    {
        private enum InventoryCategory
        {
            Elektronika,
            Meble
        }

        private readonly MyDbContext _context;

        public InventoryService(MyDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<InventoryItem>> GetAllItemsAsync()
        {
            return await _context.InventoryItems.ToListAsync();
        }

        public async Task<InventoryItem> GetItemByIdAsync(int id)
        {
            var item = await _context.InventoryItems.FindAsync(id);

            if (item == null)
            {
                throw new KeyNotFoundException($"Item with ID {id} not found.");
            }

            switch (item.itemCategory)
            {
                case nameof(InventoryCategory.Elektronika):
                    item = _context.Computers.Find(id) ?? item;
                    return item;


                case nameof(InventoryCategory.Meble):
                    item = _context.Furnitures.Find(id) ?? item;
                    return item;
                default:
                    return item;
            }
        }
    }
}
