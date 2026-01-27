using InventoryLibrary.Data;
using InventoryLibrary.Model.Inventory;
using InventoryLibrary.Model.StockTake;
using InventoryLibrary.Model.Accounts;
using InventoryLibrary.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InventoryLibrary.Model.Data;
using InventoryLibrary.Model.DTO;

namespace InventoryLibrary.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly MyDbContext _context;
    private readonly IInventoryLogger<AnalyticsService> _logger;

    public AnalyticsService(MyDbContext context, IInventoryLogger<AnalyticsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DashboardStatistics> GetDashboardStatistics(int? departmentId = null)
    {

        try
        {
            var items = await GetFilteredItems(departmentId);
            var allItems = await _context.InventoryItems
                .Include(i => i.Location)
                    .ThenInclude(r => r.Department)
                .Include(i => i.ItemType)
                .Include(i => i.ItemCondition)
                .ToListAsync();

            var accounts = await _context.Accounts.CountAsync();
            var oneYearAgo = DateTime.Now.AddYears(-1);

            var itemsWithAge = items.Where(i => i.addedDate != default).ToList();
            var averageAge = itemsWithAge.Any() 
                ? itemsWithAge.Average(i => (DateTime.Now - i.addedDate).TotalDays / 30) 
                : 0;

            return new DashboardStatistics
            {
                TotalItems = items.Count,
                BrokenItems = items.Count(i => i.ItemCondition?.Id == 3),
                ItemsOnLoan = allItems.Count(i => i.PersonInChargeId != null),
                TotalUsers = accounts,
                TotalValue = (decimal)items.Sum(i => i.itemPrice),
                DepartmentValue = (decimal)items.Sum(i => i.itemPrice),
                ItemsWithoutReview = items.Count(i => i.lastInventoryDate < oneYearAgo),
                AverageItemAge = averageAge,
                AllDepartmentsValue = (decimal)allItems.Sum(i => i.itemPrice)
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError("Error getting dashboard statistics",ex);
            throw;
        }
    }

    public async Task<MonthlyItemsCreatedData> GetMonthlyItemsCreated(int? departmentId = null)
    {
        try
        {
            var items = await GetFilteredItems(departmentId);
            var labels = new List<string>();
            var data = new List<int>();

            for (int i = 11; i >= 0; i--)
            {
                var month = DateTime.Now.AddMonths(-i);
                var monthStart = new DateTime(month.Year, month.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddSeconds(-1);
                
                labels.Add(month.ToString("MMM yyyy"));

                var monthItems = items.Count(x => 
                    x.addedDate >= monthStart && 
                    x.addedDate <= monthEnd);
                
                data.Add(monthItems);
            }

            return new MonthlyItemsCreatedData
            {
                Labels = labels,
                ItemsCreated = data,
                TotalCreated = data.Sum(),
                AveragePerMonth = data.Any() ? data.Average() : 0
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError("Error getting monthly items created", ex);
            throw;
        }
    }

    public async Task<ItemsByCategoryData> GetItemsByCategory(int? departmentId = null)
    {
        try
        {
            var items = await GetFilteredItems(departmentId);
            
            var categoryGroups = items
                .Where(i => i.ItemType != null)
                .GroupBy(i => i.ItemType!.TypeName)
                .Select(g => new CategoryInfo
                {
                    CategoryName = g.Key,
                    ItemCount = g.Count(),
                    TotalValue = (decimal)g.Sum(i => i.itemPrice),
                    Percentage = 0
                })
                .OrderByDescending(c => c.ItemCount)
                .ToList();

            var totalItems = categoryGroups.Sum(c => c.ItemCount);
            
            foreach (var category in categoryGroups)
            {
                category.Percentage = totalItems > 0 
                    ? (double)category.ItemCount / totalItems * 100 
                    : 0;
            }

            return new ItemsByCategoryData
            {
                Categories = categoryGroups,
                Labels = categoryGroups.Select(c => c.CategoryName).ToList(),
                Values = categoryGroups.Select(c => c.ItemCount).ToList(),
                TotalCategories = categoryGroups.Count
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError("Error getting items by category",ex);
            throw;
        }
    }

    public async Task<ItemsWithoutInventoryData> GetItemsWithoutInventory(int? departmentId = null)
    {
        try
        {
            var items = await GetFilteredItems(departmentId);
            var now = DateTime.Now;

            var ranges = new Dictionary<string, int>
            {
                ["0-3 miesiące"] = items.Count(i => 
                    (now - i.lastInventoryDate).TotalDays <= 90),
                ["3-6 miesięcy"] = items.Count(i => 
                    (now - i.lastInventoryDate).TotalDays > 90 && 
                    (now - i.lastInventoryDate).TotalDays <= 180),
                ["6-12 miesięcy"] = items.Count(i => 
                    (now - i.lastInventoryDate).TotalDays > 180 && 
                    (now - i.lastInventoryDate).TotalDays <= 365),
                ["Ponad rok"] = items.Count(i => 
                    (now - i.lastInventoryDate).TotalDays > 365),
                ["Ponad 2 lata"] = items.Count(i => 
                    (now - i.lastInventoryDate).TotalDays > 730)
            };

            var criticalItems = items
                .Where(i => (now - i.lastInventoryDate).TotalDays > 365)
                .OrderByDescending(i => now - i.lastInventoryDate)
                .Take(10)
                .Select(i => new ItemInventoryStatus
                {
                    ItemId = i.Id,
                    ItemName = i.itemName,
                    LastInventoryDate = i.lastInventoryDate,
                    DaysSinceInventory = (int)(now - i.lastInventoryDate).TotalDays,
                    Category = i.ItemType?.TypeName ?? "Brak kategorii"
                })
                .ToList();

            return new ItemsWithoutInventoryData
            {
                TimeRanges = ranges,
                Labels = ranges.Keys.ToList(),
                Values = ranges.Values.ToList(),
                CriticalItems = criticalItems,
                TotalOverdue = ranges["Ponad rok"]
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError("Error getting items without inventory",ex);
            throw;
        }
    }

    public async Task<MonthlyItemLossData> GetMonthlyItemLoss(int? departmentId = null)
    {
        try
        {
            var historicalItems = await _context.HistoricalItems
                .Include(h => h.ItemType)
                .ToListAsync();

            var labels = new List<string>();
            var lostData = new List<int>();
            var valueData = new List<decimal>();

            for (int i = 11; i >= 0; i--)
            {
                var month = DateTime.Now.AddMonths(-i);
                var monthStart = new DateTime(month.Year, month.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddSeconds(-1);
                
                labels.Add(month.ToString("MMM yyyy"));

                var monthLostItems = historicalItems
                    .Where(x => x.archivedDate >= monthStart && x.archivedDate <= monthEnd)
                    .ToList();

                lostData.Add(monthLostItems.Count);
                valueData.Add((decimal)monthLostItems.Sum(x => x.itemPrice));
            }

            var topLostCategories = historicalItems
                .Where(h => h.ItemType != null)
                .GroupBy(h => h.ItemType!.TypeName)
                .Select(g => new CategoryLossInfo
                {
                    CategoryName = g.Key,
                    ItemsLost = g.Count(),
                    TotalValueLost = (decimal)g.Sum(i => i.itemPrice)
                })
                .OrderByDescending(c => c.ItemsLost)
                .Take(5)
                .ToList();

            var avgLossValue = historicalItems.Any() 
                ? (decimal)historicalItems.Average(h => h.itemPrice) 
                : 0;

            var avgMonthlyLoss = lostData.Any() ? lostData.Average() : 0;

            return new MonthlyItemLossData
            {
                Labels = labels,
                ItemsLost = lostData,
                ValueLost = valueData,
                TotalLost = historicalItems.Count,
                TotalValueLost = (decimal)historicalItems.Sum(h => h.itemPrice),
                TopLostCategories = topLostCategories,
                AverageLossValue = avgLossValue,
                AverageMonthlyLoss = avgMonthlyLoss
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError("Error getting monthly item loss",ex);
            throw;
        }
    }

    public async Task<ItemConditionDistributionData> GetItemConditionDistribution(int? departmentId = null)
    {
        try
        {
            var items = await GetFilteredItems(departmentId);
            
            var conditionGroups = items
                .Where(i => i.ItemCondition != null)
                .GroupBy(i => i.ItemCondition!.ConditionName)
                .Select(g => new ConditionInfo
                {
                    ConditionName = g.Key,
                    ItemCount = g.Count(),
                    TotalValue = (decimal)g.Sum(i => i.itemPrice)
                })
                .OrderByDescending(c => c.ItemCount)
                .ToList();

            return new ItemConditionDistributionData
            {
                Conditions = conditionGroups,
                Labels = conditionGroups.Select(c => c.ConditionName).ToList(),
                Values = conditionGroups.Select(c => c.ItemCount).ToList(),
                BrokenCount = items.Count(i => i.ItemCondition?.Id == 3),
                LostCount = items.Count(i => i.ItemCondition?.Id == 4)
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError("Error getting item condition distribution",ex);
            throw;
        }
    }

    public async Task<CategoryValueData> GetCategoryValueAnalysis(int? departmentId = null)
    {
        try
        {
            var items = await GetFilteredItems(departmentId);
            
            var categoryValues = items
                .Where(i => i.ItemType != null)
                .GroupBy(i => i.ItemType!.TypeName)
                .Select(g => new CategoryValueInfo
                {
                    CategoryName = g.Key,
                    TotalValue = (decimal)g.Sum(i => i.itemPrice),
                    ItemCount = g.Count(),
                    AverageValue = g.Any() ? (decimal)g.Average(i => i.itemPrice) : 0
                })
                .OrderByDescending(c => c.TotalValue)
                .ToList();

            return new CategoryValueData
            {
                Categories = categoryValues,
                Labels = categoryValues.Select(c => c.CategoryName).ToList(),
                Values = categoryValues.Select(c => c.TotalValue).ToList(),
                TotalValue = categoryValues.Sum(c => c.TotalValue)
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError("Error getting category value analysis",ex);
            throw;
        }
    }

    public async Task<AssetValueTrendData> GetAssetValueTrend(int? departmentId = null)
    {
        try
        {
            var items = await GetFilteredItems(departmentId);
            var labels = new List<string>();
            var values = new List<decimal>();

            for (int i = 11; i >= 0; i--)
            {
                var month = DateTime.Now.AddMonths(-i);
                var monthEnd = new DateTime(month.Year, month.Month, DateTime.DaysInMonth(month.Year, month.Month), 23, 59, 59);
                
                labels.Add(month.ToString("MMM yyyy"));
                
                var valueUpToMonth = items
                    .Where(x => x.addedDate <= monthEnd)
                    .Sum(x => x.itemPrice);
                
                values.Add((decimal)valueUpToMonth);
            }

            var growthRate = values.Count > 1 && values.First() != 0
                ? ((values.Last() - values.First()) / values.First()) * 100 
                : 0;

            return new AssetValueTrendData
            {
                Labels = labels,
                Values = values,
                CurrentValue = values.LastOrDefault(),
                GrowthRate = growthRate
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError("Error getting asset value trend",ex);
            throw;
        }
    }

    private async Task<List<InventoryItem>> GetFilteredItems(int? departmentId)
    {
        var query = _context.InventoryItems
            .Include(i => i.Location)
                .ThenInclude(r => r.Department)
            .Include(i => i.ItemType)
            .Include(i => i.ItemCondition)
            .Include(i => i.personInCharge)
            .AsQueryable();

        if (departmentId.HasValue && departmentId.Value > 0)
        {
            query = query.Where(i => i.Location != null && i.Location.DepartmentId == departmentId.Value);
        }

        return await query.ToListAsync();
    }
}