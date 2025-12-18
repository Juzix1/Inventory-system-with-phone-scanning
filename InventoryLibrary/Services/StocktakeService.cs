using InventoryLibrary.Data;
using InventoryLibrary.Model.Inventory;
using InventoryLibrary.Model.StockTake;
using InventoryLibrary.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryLibrary.Services;

public class StocktakeService : IStocktakeService
{
    private readonly MyDbContext _context;
    // private readonly 
    public StocktakeService(MyDbContext context)
    {
        _context = context;
    }
    public async Task CreateNewStocktake(List<InventoryItem> items, DateTime? startDate, DateTime endDate)
    {
        try
        {
            Stocktake stock = new Stocktake
            {
                CreatedDate = DateTime.Now,
                StartDate = startDate??DateTime.Now,
                EndDate = endDate,
                AllItems = items.Count(),
            };

            _context.Stocktakes.Add(stock);
            await _context.SaveChangesAsync();
        }catch(Exception e)
        {
            throw;
        }
    }

    public async Task<IEnumerable<Stocktake>> GetAllStocktakesAsync()
    {
        try
        {
            var stocktakes = await _context.Stocktakes
                .Include(i => i.ItemsToCheck)
                .Include(i => i.AuthorizedAccounts)
                .ToListAsync();
            return stocktakes;
        }catch(Exception ex)
        {
            throw;
        }
    }
    public async Task<Stocktake> GetStocktakeById(int id)
    {
        try
        {
            var stocktake = await _context.Stocktakes.FindAsync(id);
            return stocktake;
        }catch(Exception e)
        {
            throw;
        }
    }

    public async Task<Stocktake> UpdateStocktake(Stocktake stocktake)
    {
        try
        {
            var currentStock = await _context.Stocktakes.FindAsync(stocktake.Id) ?? throw new KeyNotFoundException($"Stocktake with ID {stocktake.Id} not found.");

            _context.Entry(currentStock).CurrentValues.SetValues(stocktake);
            await _context.SaveChangesAsync();
            return currentStock;
        }catch(Exception e)
        {
            throw;
        }


    }

    public async Task MarkItem(int id,InventoryItem item)
    {
        try
        {
            var existingStocktake = await _context.Stocktakes.FindAsync(id) ?? throw new KeyNotFoundException($"Stocktake with ID {item.Id} not found.");
            existingStocktake.CheckedItemIdList.Add(item.Id);

            await _context.SaveChangesAsync();
            //aktualizacja ostatniej inwentaryzacji przedmiotu
        }catch(Exception e)
        {
            throw;
        }
    }

    public async Task<Stocktake> DeleteStocktakeAsync(int id)
    {
        try
        {
            var stocktake = await _context.Stocktakes.FindAsync(id)??throw new KeyNotFoundException($"Stocktake with ID {id} not found.");
            _context.Stocktakes.Remove(stocktake);
            await _context.SaveChangesAsync();
            return stocktake;
        }catch(Exception e)
        {
            throw;
        }
    }
}
