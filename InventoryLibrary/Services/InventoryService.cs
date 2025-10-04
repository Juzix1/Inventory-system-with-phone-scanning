using InventoryLibrary.Data;
using InventoryLibrary.Model.Inventory;
using InventoryLibrary.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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
            var items = await _context.InventoryItems
                .Include(i => i.Location)
                    .ThenInclude(r => r.Department)
                .Include(i => i.ItemType)
                .ToListAsync();
            return items;
        }

        public async Task<InventoryItem> GetItemByIdAsync(int id)
        {
            var item = await _context.InventoryItems.FindAsync(id);

            return item;
        }


    }
}
