using InventoryLibrary.Data;
using InventoryLibrary.Model.Inventory;
using InventoryLibrary.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryLibrary.Services;

public class ConditionService : IConditionService
{

    private readonly MyDbContext _context;
    public ConditionService(MyDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ItemCondition>> GetAllItemConditions()
    {

        if (!await _context.ItemTypes.AnyAsync())
        {
            return new List<ItemCondition>();
        }
        return await _context.itemConditions.ToListAsync();
;
    }

}
