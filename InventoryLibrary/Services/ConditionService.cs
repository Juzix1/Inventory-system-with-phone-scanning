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
        var conditions = await _context.itemConditions.ToListAsync();

        return conditions;
    }

}
