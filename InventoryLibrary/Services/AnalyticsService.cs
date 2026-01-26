using InventoryLibrary.Data;
using InventoryLibrary.Model.Inventory;
using InventoryLibrary.Model.StockTake;
using InventoryLibrary.Model.Accounts;
using InventoryLibrary.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryLibrary.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly MyDbContext _context;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(MyDbContext context, ILogger<AnalyticsService> logger)
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
                ItemsOnLoan = items.Count(i => i.PersonInChargeId != null),
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
            _logger?.LogError(ex, "Error getting dashboard statistics");
            throw;
        }
    }

    // Nowy: Przedmioty utworzone miesięcznie (ostatnie 12 miesięcy)
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
                labels.Add(month.ToString("MMM yyyy"));

                var monthItems = items.Count(x => 
                    x.addedDate.Year == month.Year && 
                    x.addedDate.Month == month.Month);
                
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
            _logger?.LogError(ex, "Error getting monthly items created");
            throw;
        }
    }

    // Nowy: Rozkład przedmiotów według kategorii (dla PieChart)
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
                    Percentage = 0 // Zostanie obliczone poniżej
                })
                .OrderByDescending(c => c.ItemCount)
                .ToList();

            var totalItems = categoryGroups.Sum(c => c.ItemCount);
            
            // Oblicz procentowy udział
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
            _logger?.LogError(ex, "Error getting items by category");
            throw;
        }
    }

    // Nowy: Przedmioty bez inwentaryzacji (przekraczające określony próg)
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
            _logger?.LogError(ex, "Error getting items without inventory");
            throw;
        }
    }

    // Poprawiona: Utrata przedmiotów miesięcznie (usunięte/zutylizowane przedmioty)
public async Task<MonthlyItemLossData> GetMonthlyItemLoss(int? departmentId = null)
{
    try
    {
        // Pobierz wszystkie historyczne przedmioty (usunięte/zutylizowane)
        var historicalItems = await _context.HistoricalItems
            .Include(h => h.ItemType)
            .ToListAsync();

        // Jeśli filtrujemy po departamencie, musimy to uwzględnić
        // (zakładam że HistoricalItem nie ma bezpośredniej relacji do Department,
        // więc możemy pominąć filtrowanie lub dodać pole DepartmentId do HistoricalItem)
        
        var labels = new List<string>();
        var lostData = new List<int>();
        var valueData = new List<decimal>();

        // Zliczaj przedmioty usunięte w każdym miesiącu
        for (int i = 11; i >= 0; i--)
        {
            var month = DateTime.Now.AddMonths(-i);
            var monthStart = new DateTime(month.Year, month.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            
            labels.Add(month.ToString("MMM yyyy"));

            // Przedmioty zarchiwizowane w tym miesiącu
            var monthLostItems = historicalItems
                .Where(x => x.archivedDate >= monthStart && x.archivedDate <= monthEnd)
                .ToList();

            lostData.Add(monthLostItems.Count);
            valueData.Add((decimal)monthLostItems.Sum(x => x.itemPrice));
        }

        // Top kategorie z największą stratą (przez cały czas)
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

        // Dodatkowa analiza - średnia wartość utraconego przedmiotu
        var avgLossValue = historicalItems.Any() 
            ? (decimal)historicalItems.Average(h => h.itemPrice) 
            : 0;

        // Miesięczna średnia utraty
        var avgMonthlyLoss = lostData.Any() ? lostData.Average() : 0;

        return new MonthlyItemLossData
        {
            Labels = labels,
            ItemsLost = lostData,
            ValueLost = valueData,
            TotalLost = historicalItems.Count, // Wszystkie usunięte przedmioty
            TotalValueLost = (decimal)historicalItems.Sum(h => h.itemPrice),
            TopLostCategories = topLostCategories,
            AverageLossValue = avgLossValue,
            AverageMonthlyLoss = avgMonthlyLoss
        };
    }
    catch (Exception ex)
    {
        _logger?.LogError(ex, "Error getting monthly item loss");
        throw;
    }
}

    // Nowy: Stan przedmiotów (zdrowe, uszkodzone, zagubione, etc.)
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
            _logger?.LogError(ex, "Error getting item condition distribution");
            throw;
        }
    }

    // Nowy: Wartość przedmiotów według kategorii (dla analizy budżetu)
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
            _logger?.LogError(ex, "Error getting category value analysis");
            throw;
        }
    }

    // Nowy: Trend wartości aktywów w czasie
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
                labels.Add(month.ToString("MMM yyyy"));
                
                var valueUpToMonth = items
                    .Where(x => x.addedDate <= month)
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
            _logger?.LogError(ex, "Error getting asset value trend");
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
            .AsQueryable();

        if (departmentId.HasValue && departmentId.Value > 0)
        {
            query = query.Where(i => i.Location != null && i.Location.DepartmentId == departmentId.Value);
        }

        return await query.ToListAsync();
    }
}

// ==================== DTOs ====================

public class DashboardStatistics
{
    public int TotalItems { get; set; }
    public int BrokenItems { get; set; }
    public int ItemsOnLoan { get; set; }
    public int TotalUsers { get; set; }
    public decimal TotalValue { get; set; }
    public decimal DepartmentValue { get; set; }
    public int ItemsWithoutReview { get; set; }
    public double AverageItemAge { get; set; }
    public decimal AllDepartmentsValue { get; set; }
}

public class MonthlyItemsCreatedData
{
    public List<string> Labels { get; set; } = new();
    public List<int> ItemsCreated { get; set; } = new();
    public int TotalCreated { get; set; }
    public double AveragePerMonth { get; set; }
}

public class ItemsByCategoryData
{
    public List<CategoryInfo> Categories { get; set; } = new();
    public List<string> Labels { get; set; } = new();
    public List<int> Values { get; set; } = new();
    public int TotalCategories { get; set; }
}

public class CategoryInfo
{
    public string CategoryName { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public decimal TotalValue { get; set; }
    public double Percentage { get; set; }
}

public class ItemsWithoutInventoryData
{
    public Dictionary<string, int> TimeRanges { get; set; } = new();
    public List<string> Labels { get; set; } = new();
    public List<int> Values { get; set; } = new();
    public List<ItemInventoryStatus> CriticalItems { get; set; } = new();
    public int TotalOverdue { get; set; }
}

public class ItemInventoryStatus
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public DateTime LastInventoryDate { get; set; }
    public int DaysSinceInventory { get; set; }
    public string Category { get; set; } = string.Empty;
}

public class MonthlyItemLossData
{
    public List<string> Labels { get; set; } = new();
    public List<int> ItemsLost { get; set; } = new();
    public List<decimal> ValueLost { get; set; } = new();
    public int TotalLost { get; set; }
    public decimal TotalValueLost { get; set; }
    public List<CategoryLossInfo> TopLostCategories { get; set; } = new();
    public decimal AverageLossValue { get; set; } // NOWE
    public double AverageMonthlyLoss { get; set; } // NOWE
}

public class CategoryLossInfo
{
    public string CategoryName { get; set; } = string.Empty;
    public int ItemsLost { get; set; }
    public decimal TotalValueLost { get; set; }
}

public class ItemConditionDistributionData
{
    public List<ConditionInfo> Conditions { get; set; } = new();
    public List<string> Labels { get; set; } = new();
    public List<int> Values { get; set; } = new();
    public int BrokenCount { get; set; }
    public int LostCount { get; set; }
}

public class ConditionInfo
{
    public string ConditionName { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public decimal TotalValue { get; set; }
}

public class CategoryValueData
{
    public List<CategoryValueInfo> Categories { get; set; } = new();
    public List<string> Labels { get; set; } = new();
    public List<decimal> Values { get; set; } = new();
    public decimal TotalValue { get; set; }
}

public class CategoryValueInfo
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalValue { get; set; }
    public int ItemCount { get; set; }
    public decimal AverageValue { get; set; }
}

public class AssetValueTrendData
{
    public List<string> Labels { get; set; } = new();
    public List<decimal> Values { get; set; } = new();
    public decimal CurrentValue { get; set; }
    public decimal GrowthRate { get; set; }
}