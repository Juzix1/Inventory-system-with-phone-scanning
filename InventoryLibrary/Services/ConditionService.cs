using InventoryLibrary.Data;
using InventoryLibrary.Model.Inventory;
using InventoryLibrary.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryLibrary.Services;

public class ConditionService : IConditionService
{

    private readonly MyDbContext _context;
    private readonly IInventoryLogger<ConditionService> _logger;
    public ConditionService(MyDbContext context, IInventoryLogger<ConditionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ItemCondition>> GetAllItemConditions()
    {

        try
        {
            if (!await _context.ItemTypes.AnyAsync())
            {
                return new List<ItemCondition>();
            }
            return await _context.itemConditions.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error while getting conditions info", ex);
            throw;
        }
    }

}
