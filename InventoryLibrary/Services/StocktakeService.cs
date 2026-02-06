using InventoryLibrary.Data;
using InventoryLibrary.Model.Accounts;
using InventoryLibrary.Model.DTO.Stocktake;
using InventoryLibrary.Model.Inventory;
using InventoryLibrary.Model.StockTake;
using InventoryLibrary.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace InventoryLibrary.Services;

public class StocktakeService : IStocktakeService
{
    private readonly MyDbContext _context;
    private readonly IInventoryLogger<StocktakeService> _logger;

    public StocktakeService(MyDbContext context, IInventoryLogger<StocktakeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task CreateNewStocktake(string name,string description,List<InventoryItem> items,List<Account> authorizedAccounts,DateTime startDate,DateTime endDate)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Stocktake name is required", nameof(name));
            if (items == null || !items.Any())
                throw new ArgumentException("You must choose at least one item for stocktake", nameof(items));
            if (endDate <= startDate)
                throw new ArgumentException("End date must be after start date", nameof(endDate));
            var stocktake = new Stocktake
            {
                Name = name,
                Description = description,
                CreatedDate = DateTime.Now,
                StartDate = startDate,
                EndDate = endDate,
                Status = startDate <= DateTime.Now ? StockTakeStatus.InProgress : StockTakeStatus.Planned,
                AllItems = items.Count,
                ItemsToCheck = items,
                AuthorizedAccounts = authorizedAccounts ?? new List<Account>(),
                CheckedItems = new List<StocktakeCheckedItem>()
            };
            _context.Stocktakes.Add(stocktake);
            await _context.SaveChangesAsync();
            _logger?.LogInfo($"Created new stocktake: {name} with {items.Count} items.");
        }
        catch (Exception ex)
        {
            _logger?.LogError("Error while creating new stocktake", ex);
            throw;
        }
    }
    public async Task CreateNewStocktake(List<InventoryItem> items, DateTime? startDate, DateTime endDate)
    {
        await CreateNewStocktake(
            name: $"Stocktake {DateTime.Now:yyyy-MM-dd}",
            description: "Auto-generated stocktake",
            items: items,
            authorizedAccounts: new List<Account>(),
            startDate: startDate ?? DateTime.Now,
            endDate: endDate
        );
    }

    public async Task<IEnumerable<Stocktake>> GetAllStocktakesAsync()
    {
        try
        {
            var stocktakes = await _context.Stocktakes
                .Include(s => s.ItemsToCheck)
                    .ThenInclude(i => i.Location)
                .Include(s => s.ItemsToCheck)
                    .ThenInclude(i => i.ItemType)
                .Include(s => s.CheckedItems)
                .Include(s => s.AuthorizedAccounts)
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();

            return stocktakes;
        }
        catch (Exception ex)
        {
            _logger?.LogError("Error while getting stocktakes info", ex);
            throw;
        }
    }

    public async Task<IEnumerable<Stocktake>> GetStocktakesByAccount(int id)
    {
        try
        {
            var stocktakes = await _context.Stocktakes
                .AsNoTracking()
                .Include(s => s.ItemsToCheck)
                    .ThenInclude(i => i.Location)
                .Include(s => s.ItemsToCheck)
                    .ThenInclude(i => i.ItemType)
                .Include(s => s.ItemsToCheck)
                    .ThenInclude(i => i.ItemCondition)
                .Include(s => s.ItemsToCheck)
                    .ThenInclude(i => i.Location.Department)
                .Include(s => s.CheckedItems)
                .Include(s => s.AuthorizedAccounts)
                .Where(i => i.EndDate > DateTime.Now)
                .Where(a => a.AuthorizedAccounts.Any(a => a.Id == id))
                .ToListAsync();
            
            return stocktakes;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error while getting stocktakes for account {id}", ex);
            return new List<Stocktake>();
        }
    }

    public async Task<Stocktake> GetStocktakeById(int id)
    {
        try
        {
            var stocktake = await _context.Stocktakes
                .Include(s => s.ItemsToCheck)
                    .ThenInclude(i => i.Location)
                .Include(s => s.ItemsToCheck)
                    .ThenInclude(i => i.ItemType)
                .Include(s => s.ItemsToCheck)
                    .ThenInclude(i => i.ItemCondition)
                .Include(s => s.ItemsToCheck)
                    .ThenInclude(i => i.personInCharge)
                .Include(s => s.CheckedItems)
                .Include(s => s.AuthorizedAccounts)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (stocktake == null)
                throw new KeyNotFoundException($"Stocktake with id {id} not found");

            return stocktake;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error while getting info for stocktake {id}", ex);
            throw;
        }
    }

    public async Task<Stocktake> UpdateStocktake(Stocktake stocktake)
    {
        try
        {
            if (stocktake == null)
                throw new ArgumentNullException(nameof(stocktake));

            var existingStocktake = await _context.Stocktakes
                .Include(s => s.ItemsToCheck)
                .Include(s => s.CheckedItems)
                .Include(s => s.AuthorizedAccounts)
                .FirstOrDefaultAsync(s => s.Id == stocktake.Id);

            if (existingStocktake == null)
                throw new KeyNotFoundException($"Stocktake with Id {stocktake.Id} not found");

            existingStocktake.Name = stocktake.Name;
            existingStocktake.Description = stocktake.Description;
            existingStocktake.StartDate = stocktake.StartDate;
            existingStocktake.EndDate = stocktake.EndDate;
            existingStocktake.Status = stocktake.Status;

            await _context.SaveChangesAsync();

            _logger?.LogInfo($"Stocktake Updated with id {stocktake.Id}");
            return existingStocktake;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error while updating stocktake: {stocktake?.Id}", ex);
            throw;
        }
    }
    public async Task<List<int>> GetAssignedItemsIds(int stocktakeId)
    {
        try
        {

            var existingStocktake = await _context.Stocktakes.FirstOrDefaultAsync(s => s.Id == stocktakeId);

            if (existingStocktake == null)
                throw new KeyNotFoundException($"Stocktake with Id {stocktakeId} not found");
            
            var items = await _context.StocktakeCheckedItems
                .Where(i => i.StocktakeId == stocktakeId)
                .ToListAsync();
            List<int> indexes = new();
            foreach (var chkItem in items)
            {
                indexes.Add(chkItem.InventoryItemId);
            }
            return indexes;
        }catch(Exception ex)
        {
            _logger.LogError(ex.Message);
            return new List<int>();
            
        }
    }

    public async Task MarkItemAsChecked(int stocktakeId, int itemId, string checkedBy = null)
    {
        try
        {
            var stocktake = await _context.Stocktakes
                .Include(s => s.ItemsToCheck)
                .Include(s => s.CheckedItems)
                .FirstOrDefaultAsync(s => s.Id == stocktakeId);

            if (stocktake == null)
                throw new KeyNotFoundException($"Stocktake with id {stocktakeId} not found");

            if (stocktake.Status != StockTakeStatus.InProgress)
                throw new InvalidOperationException("Cannot mark items in a stocktake that is not in progress");

            var item = stocktake.ItemsToCheck.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
                throw new KeyNotFoundException($"Item with id {itemId} not found in stocktake {stocktakeId}");

            var existingCheck = await _context.StocktakeCheckedItems
                .FirstOrDefaultAsync(ci => ci.StocktakeId == stocktakeId && ci.InventoryItemId == itemId);

            if (existingCheck == null)
            {
                var checkedItem = new StocktakeCheckedItem
                {
                    StocktakeId = stocktakeId,
                    InventoryItemId = itemId,
                    CheckedDate = DateTime.Now,
                    CheckedByUserId = ParseUserId(checkedBy)
                };

                _context.StocktakeCheckedItems.Add(checkedItem);
                item.lastInventoryDate = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger?.LogInfo($"Marked item {itemId} as checked in stocktake {stocktakeId}" +
                                 (checkedBy != null ? $" by {checkedBy}" : ""));

                var checkedCount = await GetCheckedItemsCount(stocktakeId);
                if (checkedCount == stocktake.AllItems)
                {
                    await AutoCompleteStocktake(stocktakeId);
                }
            }
            else
            {
                _logger?.LogInfo($"Item {itemId} already checked in stocktake {stocktakeId}");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error while marking item {itemId} as checked in stocktake {stocktakeId}", ex);
            throw;
        }
    }

    public async Task MarkItem(int id, InventoryItem item)
    {
        await MarkItemAsChecked(id, item.Id);
    }

    public async Task UnmarkItemAsChecked(int stocktakeId, int itemId)
    {
        try
        {
            var checkedItem = await _context.StocktakeCheckedItems
                .FirstOrDefaultAsync(ci => ci.StocktakeId == stocktakeId && ci.InventoryItemId == itemId);

            if (checkedItem != null)
            {
                _context.StocktakeCheckedItems.Remove(checkedItem);
                await _context.SaveChangesAsync();

                _logger?.LogInfo($"Unmarked item {itemId} as checked in stocktake {stocktakeId}");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error while unmarking item {itemId} as checked in stocktake {stocktakeId}", ex);
            throw;
        }
    }

    private async Task AutoCompleteStocktake(int stocktakeId)
    {
        try
        {
            var stocktake = await _context.Stocktakes.FindAsync(stocktakeId);
            if (stocktake != null && stocktake.Status == StockTakeStatus.InProgress)
            {
                stocktake.Status = StockTakeStatus.Completed;
                stocktake.EndDate = DateTime.Now;
                await _context.SaveChangesAsync();

                _logger?.LogInfo($"Auto-completed stocktake {stocktakeId} as all items have been checked.");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error while auto-completing stocktake {stocktakeId}", ex);
        }
    }

    public async Task<Stocktake> DeleteStocktakeAsync(int id)
    {
        try
        {
            var stocktake = await _context.Stocktakes
                .Include(s => s.ItemsToCheck)
                .Include(s => s.CheckedItems)
                .Include(s => s.AuthorizedAccounts)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (stocktake == null)
                throw new KeyNotFoundException($"Stocktake with id {id} not found");

            _context.Stocktakes.Remove(stocktake);
            await _context.SaveChangesAsync();

            _logger?.LogWarning($"Deleted stocktake with id {id}");
            return stocktake;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error while deleting stocktake {id}", ex);
            throw;
        }
    }

    public async Task<StocktakeStatistics> GetStocktakeStatistics(int stocktakeId)
    {
        try
        {
            var stocktake = await GetStocktakeById(stocktakeId);
            var checkedCount = stocktake.CheckedItems.Count;

            var stats = new StocktakeStatistics
            {
                TotalItems = stocktake.AllItems,
                CheckedItems = checkedCount,
                UncheckedItems = stocktake.AllItems - checkedCount,
                ProgressPercentage = stocktake.AllItems > 0
                    ? (double)checkedCount / stocktake.AllItems * 100
                    : 0,
                DaysRemaining = (stocktake.EndDate - DateTime.Now).Days,
                IsOverdue = DateTime.Now > stocktake.EndDate,
                AverageItemsPerDay = CalculateAverageItemsPerDay(stocktake)
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error while getting statistics for stocktake {stocktakeId}", ex);
            throw;
        }
    }

    private double CalculateAverageItemsPerDay(Stocktake stocktake)
    {
        var daysPassed = (DateTime.Now - stocktake.StartDate).Days;
        if (daysPassed <= 0) return 0;

        return (double)stocktake.CheckedItems.Count / daysPassed;
    }

    public async Task<List<InventoryItem>> GetUncheckedItems(int stocktakeId)
    {
        try
        {
            var stocktake = await GetStocktakeById(stocktakeId);
            var checkedItemIds = stocktake.CheckedItems.Select(ci => ci.InventoryItemId).ToHashSet();

            return stocktake.ItemsToCheck
                .Where(item => !checkedItemIds.Contains(item.Id))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error while getting unchecked items for stocktake {stocktakeId}", ex);
            throw;
        }
    }

    public async Task<bool> IsUserAuthorized(int stocktakeId, int userId)
    {
        try
        {
            var stocktake = await _context.Stocktakes
                .Include(s => s.AuthorizedAccounts)
                .FirstOrDefaultAsync(s => s.Id == stocktakeId);

            if (stocktake == null) return false;

            return stocktake.AuthorizedAccounts.Any(a => a.Id == userId);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error while checking user authorization for stocktake {stocktakeId}", ex);
            throw;
        }
    }
    public async Task<List<StocktakeCheckedItem>> GetCheckedItemsDetails(int stocktakeId)
    {
        try
        {
            return await _context.StocktakeCheckedItems
                .Where(ci => ci.StocktakeId == stocktakeId)
                .OrderByDescending(ci => ci.CheckedDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error while getting checked items details for stocktake {stocktakeId}", ex);
            throw;
        }
    }

    public async Task<bool> IsItemChecked(int stocktakeId, int itemId)
    {
        try
        {
            return await _context.StocktakeCheckedItems
                .AnyAsync(ci => ci.StocktakeId == stocktakeId && ci.InventoryItemId == itemId);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error while checking if item {itemId} is checked in stocktake {stocktakeId}", ex);
            throw;
        }
    }

    public async Task MarkMultipleItemsAsChecked(int stocktakeId, List<int> itemIds, string checkedBy = null)
    {
        try
        {
            var stocktake = await _context.Stocktakes
                .Include(s => s.ItemsToCheck)
                .Include(s => s.CheckedItems)
                .FirstOrDefaultAsync(s => s.Id == stocktakeId);

            if (stocktake == null)
                throw new KeyNotFoundException($"Stocktake with id {stocktakeId} not found");

            if (stocktake.Status != StockTakeStatus.InProgress)
                throw new InvalidOperationException("Cannot mark items in a stocktake that is not in progress");

            var existingCheckedIds = await _context.StocktakeCheckedItems
                .Where(ci => ci.StocktakeId == stocktakeId && itemIds.Contains(ci.InventoryItemId))
                .Select(ci => ci.InventoryItemId)
                .ToListAsync();

            var itemsToCheck = itemIds.Except(existingCheckedIds).ToList();

            if (itemsToCheck.Any())
            {
                var userId = ParseUserId(checkedBy);
                var checkedItems = itemsToCheck.Select(itemId => new StocktakeCheckedItem
                {
                    StocktakeId = stocktakeId,
                    InventoryItemId = itemId,
                    CheckedDate = DateTime.Now,
                    CheckedByUserId = userId
                }).ToList();

                await _context.StocktakeCheckedItems.AddRangeAsync(checkedItems);

                var items = await _context.InventoryItems
                    .Where(i => itemsToCheck.Contains(i.Id))
                    .ToListAsync();

                foreach (var item in items)
                {
                    item.lastInventoryDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                _logger?.LogInfo($"Marked {itemsToCheck.Count} items as checked in stocktake {stocktakeId}" +
                                 (checkedBy != null ? $" by {checkedBy}" : ""));

                var totalChecked = await GetCheckedItemsCount(stocktakeId);
                if (totalChecked == stocktake.AllItems)
                {
                    await AutoCompleteStocktake(stocktakeId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error while marking multiple items as checked in stocktake {stocktakeId}", ex);
            throw;
        }
    }

    public async Task UnmarkMultipleItemsAsChecked(int stocktakeId, List<int> itemIds)
    {
        try
        {
            var checkedItems = await _context.StocktakeCheckedItems
                .Where(ci => ci.StocktakeId == stocktakeId && itemIds.Contains(ci.InventoryItemId))
                .ToListAsync();

            if (checkedItems.Any())
            {
                _context.StocktakeCheckedItems.RemoveRange(checkedItems);
                await _context.SaveChangesAsync();

                _logger?.LogInfo($"Unmarked {checkedItems.Count} items in stocktake {stocktakeId}");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error while unmarking multiple items in stocktake {stocktakeId}", ex);
            throw;
        }
    }

    public async Task<int> GetCheckedItemsCount(int stocktakeId)
    {
        try
        {
            return await _context.StocktakeCheckedItems
                .CountAsync(ci => ci.StocktakeId == stocktakeId);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error while getting checked items count for stocktake {stocktakeId}", ex);
            throw;
        }
    }

    public async Task<int> GetProgressPercentage(int stocktakeId)
    {
        try
        {
            var stocktake = await _context.Stocktakes
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == stocktakeId);

            if (stocktake == null || stocktake.AllItems == 0) return 0;

            var checkedCount = await GetCheckedItemsCount(stocktakeId);
            return (int)((double)checkedCount / stocktake.AllItems * 100);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error while getting progress percentage for stocktake {stocktakeId}", ex);
            throw;
        }
    }

    private int? ParseUserId(string checkedBy)
    {
        if (string.IsNullOrWhiteSpace(checkedBy))
            return null;

        if (int.TryParse(checkedBy, out int userId))
            return userId;

        return null;
    }
}