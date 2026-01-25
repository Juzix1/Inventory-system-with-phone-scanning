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

    public async Task<RevenueAnalysis> GetRevenueAnalysis(int? departmentId = null)
    {
        try
        {
            var items = await GetFilteredItems(departmentId);
            var labels = new List<string>();
            var revenueData = new List<decimal>();
            var damageData = new List<decimal>();

            for (int i = 0; i < 12; i++)
            {
                var month = DateTime.Now.AddMonths(-11 + i);
                labels.Add(month.ToString("MMM yyyy"));

                var monthItems = items.Where(x => x.addedDate.Year == month.Year && x.addedDate.Month == month.Month);
                var revenue = monthItems.Sum(x => x.itemPrice);
                var damage = monthItems.Where(x => x.ItemCondition?.Id == 3).Sum(x => x.itemPrice);

                revenueData.Add((decimal)revenue);
                damageData.Add((decimal)damage);
            }

            return new RevenueAnalysis
            {
                Labels = labels,
                RevenueData = revenueData,
                DamageData = damageData,
                TotalRevenue = revenueData.Sum(),
                TotalDamage = damageData.Sum()
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting revenue analysis");
            throw;
        }
    }

    public async Task<ItemsOverTimeData> GetItemsOverTimeData(int? departmentId = null)
    {
        try
        {
            var items = await GetFilteredItems(departmentId);
            var labels = new List<string>();
            var data = new List<int>();

            for (int i = 0; i < 30; i++)
            {
                var date = DateTime.Now.AddDays(-29 + i);
                labels.Add(date.ToString("dd MMM"));
                data.Add(items.Count(x => x.addedDate <= date));
            }

            return new ItemsOverTimeData
            {
                Labels = labels,
                Data = data
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting items over time data");
            throw;
        }
    }

    public async Task<DamageAnalysis> GetDamageByMonthData(int? departmentId = null)
    {
        try
        {
            var items = await GetFilteredItems(departmentId);
            var labels = new List<string>();
            var data = new List<int>();

            for (int i = 0; i < 12; i++)
            {
                var month = DateTime.Now.AddMonths(-11 + i);
                labels.Add(month.ToString("MMM yyyy"));
                data.Add(items.Count(x => x.ItemCondition?.Id == 3 && 
                    x.addedDate.Year == month.Year && 
                    x.addedDate.Month == month.Month));
            }

            return new DamageAnalysis
            {
                Labels = labels,
                DamageCountByMonth = data,
                TotalDamaged = items.Count(i => i.ItemCondition?.Id == 3)
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting damage by month data");
            throw;
        }
    }

    public async Task<DepartmentAssetData> GetAssetByDepartmentData()
    {
        try
        {
            var items = await _context.InventoryItems
                .Include(i => i.Location)
                    .ThenInclude(r => r.Department)
                .ToListAsync();

            var deptGroups = items
                .Where(i => i.Location?.Department != null)
                .GroupBy(i => i.Location!.Department!.DepartmentName)
                .Select(g => new DepartmentAsset
                {
                    DepartmentName = g.Key,
                    TotalValue = (decimal)g.Sum(i => i.itemPrice),
                    ItemCount = g.Count()
                })
                .OrderByDescending(x => x.TotalValue)
                .Take(10)
                .ToList();

            return new DepartmentAssetData
            {
                Departments = deptGroups,
                Labels = deptGroups.Select(d => d.DepartmentName).ToList(),
                Values = deptGroups.Select(d => d.TotalValue).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting asset by department data");
            throw;
        }
    }

    public async Task<AgeByTypeData> GetAverageAgeByTypeData(int? departmentId = null)
    {
        try
        {
            var items = await GetFilteredItems(departmentId);
            
            var typeGroups = items
                .Where(i => i.ItemType != null && i.addedDate != default)
                .GroupBy(i => i.ItemType!.TypeName)
                .Select(g => new TypeAgeInfo
                {
                    TypeName = g.Key,
                    AverageAgeMonths = g.Average(i => (DateTime.Now - i.addedDate).TotalDays / 30),
                    ItemCount = g.Count()
                })
                .ToList();

            return new AgeByTypeData
            {
                TypeAges = typeGroups,
                Labels = typeGroups.Select(t => t.TypeName).ToList(),
                AverageAges = typeGroups.Select(t => t.AverageAgeMonths).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting average age by type data");
            throw;
        }
    }

    public async Task<PurchasesVsDisposalsData> GetPurchasesVsDisposalsData(int? departmentId = null)
    {
        try
        {
            var items = await GetFilteredItems(departmentId);
            var labels = new List<string>();
            var purchases = new List<int>();
            var disposals = new List<int>();

            for (int i = 0; i < 12; i++)
            {
                var month = DateTime.Now.AddMonths(-11 + i);
                labels.Add(month.ToString("MMM yyyy"));

                purchases.Add(items.Count(x => x.addedDate.Year == month.Year && 
                    x.addedDate.Month == month.Month));
                
                disposals.Add(items.Count(x => x.ItemCondition?.Id == 5 && 
                    x.addedDate.Year == month.Year && 
                    x.addedDate.Month == month.Month));
            }

            return new PurchasesVsDisposalsData
            {
                Labels = labels,
                Purchases = purchases,
                Disposals = disposals,
                TotalPurchases = purchases.Sum(),
                TotalDisposals = disposals.Sum()
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting purchases vs disposals data");
            throw;
        }
    }

    public async Task<AssetValueTrendData> GetAssetValueTrendData(int? departmentId = null)
    {
        try
        {
            var items = await GetFilteredItems(departmentId);
            var labels = new List<string>();
            var values = new List<decimal>();

            for (int i = 0; i < 12; i++)
            {
                var month = DateTime.Now.AddMonths(-11 + i);
                labels.Add(month.ToString("MMM yyyy"));
                values.Add((decimal)items.Where(x => x.addedDate <= month).Sum(x => x.itemPrice));
            }

            var growthRate = values.Count > 1 
                ? ((values.Last() - values.First()) / (values.First() == 0 ? 1 : values.First())) * 100 
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
            _logger?.LogError(ex, "Error getting asset value trend data");
            throw;
        }
    }

    public async Task<StocktakeProgressData> GetStocktakeProgressData()
    {
        try
        {
            var stocktakes = await _context.Stocktakes
                .Include(s => s.ItemsToCheck)
                .Include(s => s.AuthorizedAccounts)
                .Where(s => s.Status == StockTakeStatus.InProgress || s.Status == StockTakeStatus.Planned)
                .OrderByDescending(s => s.CreatedDate)
                .Take(5)
                .ToListAsync();

            var progressList = stocktakes.Select(st => new StocktakeProgress
            {
                Id = st.Id,
                Name = st.Name,
                TotalItems = st.AllItems,
                CheckedItems = st.CheckedItemIdList.Count,
                ProgressPercentage = st.AllItems > 0 
                    ? (st.CheckedItemIdList.Count * 100.0 / st.AllItems) 
                    : 0,
                Status = st.Status,
                DaysRemaining = (st.EndDate - DateTime.Now).Days,
                IsOverdue = DateTime.Now > st.EndDate
            }).ToList();

            var totalStocktakes = await _context.Stocktakes.CountAsync();
            var completedStocktakes = await _context.Stocktakes
                .CountAsync(s => s.Status == StockTakeStatus.Completed);

            return new StocktakeProgressData
            {
                ActiveStocktakes = progressList,
                TotalStocktakes = totalStocktakes,
                CompletedStocktakes = completedStocktakes,
                InProgressCount = stocktakes.Count(s => s.Status == StockTakeStatus.InProgress)
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting stocktake progress data");
            throw;
        }
    }

    public async Task<DepreciationData> GetDepreciationData(int? departmentId = null)
    {
        try
        {
            var items = await GetFilteredItems(departmentId);
            
            if (!items.Any())
            {
                return new DepreciationData
                {
                    OriginalValue = 0,
                    CurrentValue = 0,
                    TotalDepreciation = 0,
                    DepreciationRate = 0,
                    AnnualDepreciation = 0
                };
            }
            
            var originalValue = items.Sum(i => (decimal)i.itemPrice);
            
            // Simplified depreciation: 5% per year
            var currentValue = items.Sum(i =>
            {
                var age = (decimal)((DateTime.Now - i.addedDate).TotalDays / 365.25);
                var depreciationRate = Math.Min(age * 0.05m, 0.7m); // Max 70% depreciation
                return (decimal)i.itemPrice * (1m - depreciationRate);
            });

            var totalDepreciation = originalValue - currentValue;
            var depreciationRate = originalValue > 0 
                ? (totalDepreciation / originalValue * 100m) 
                : 0m;
            
            var oldestItemYear = items.Min(i => i.addedDate).Year;
            var yearsDiff = Math.Max((DateTime.Now.Year - oldestItemYear), 1);

            return new DepreciationData
            {
                OriginalValue = originalValue,
                CurrentValue = currentValue,
                TotalDepreciation = totalDepreciation,
                DepreciationRate = depreciationRate,
                AnnualDepreciation = totalDepreciation / yearsDiff
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting depreciation data");
            throw;
        }
    }

    public async Task<InventoryIssuesData> GetInventoryIssuesData(int? departmentId = null)
    {
        try
        {
            var items = await GetFilteredItems(departmentId);
            
            var oneYearAgo = DateTime.Now.AddYears(-1);

            return new InventoryIssuesData
            {
                ItemsNotFound = items.Count(i => i.ItemCondition?.Id == 4), // Lost
                ItemsWithoutLocation = items.Count(i => i.RoomId == null && i.PersonInChargeId == null),
                LostItems = items.Count(i => i.ItemCondition?.Id == 4),
                DisposedItems = items.Count(i => i.ItemCondition?.Id == 5),
                ItemsWithoutReview = items.Count(i => i.lastInventoryDate < oneYearAgo),
                ItemsOutOfAssignedLocation = items.Count(i => i.RoomId == null && i.PersonInChargeId != null),
                ItemsWithMissingInventoryNumber = items.Count(i => string.IsNullOrEmpty(i.itemName)),
                ItemsNotCompliantWithPolicy = items.Count(i => i.ItemCondition?.Id == 3 || i.ItemCondition?.Id == 4)
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting inventory issues data");
            throw;
        }
    }

    public async Task<ReplacementForecastData> GetReplacementForecastData(int? departmentId = null)
    {
        try
        {
            var items = await GetFilteredItems(departmentId);
            
            if (!items.Any())
            {
                return new ReplacementForecastData
                {
                    ItemsOlderThan5Years = 0,
                    EstimatedReplacementCost = 0,
                    NextYearForecast = 0,
                    Next3YearsForecast = 0,
                    AgeDistribution = new Dictionary<string, int>
                    {
                        ["0-2 years"] = 0,
                        ["3-5 years"] = 0,
                        ["6-10 years"] = 0,
                        ["10+ years"] = 0
                    },
                    AverageReplacementCycle = 0
                };
            }
            
            var fiveYearsAgo = DateTime.Now.AddYears(-5);
            var oldItems = items.Where(i => i.addedDate < fiveYearsAgo).ToList();
            
            var itemsOlderThan5Years = oldItems.Count;
            var estimatedReplacementCost = (decimal)oldItems.Sum(i => i.itemPrice) * 1.2m; // 20% inflation
            var nextYearForecast = estimatedReplacementCost * 0.15m; // 15% expected replacement rate

            // Group by age ranges
            var ageDistribution = new Dictionary<string, int>
            {
                ["0-2 years"] = items.Count(i => (DateTime.Now - i.addedDate).TotalDays / 365.25 <= 2),
                ["3-5 years"] = items.Count(i => (DateTime.Now - i.addedDate).TotalDays / 365.25 > 2 && 
                                                  (DateTime.Now - i.addedDate).TotalDays / 365.25 <= 5),
                ["6-10 years"] = items.Count(i => (DateTime.Now - i.addedDate).TotalDays / 365.25 > 5 && 
                                                   (DateTime.Now - i.addedDate).TotalDays / 365.25 <= 10),
                ["10+ years"] = items.Count(i => (DateTime.Now - i.addedDate).TotalDays / 365.25 > 10)
            };

            return new ReplacementForecastData
            {
                ItemsOlderThan5Years = itemsOlderThan5Years,
                EstimatedReplacementCost = estimatedReplacementCost,
                NextYearForecast = nextYearForecast,
                Next3YearsForecast = nextYearForecast * 3,
                AgeDistribution = ageDistribution,
                AverageReplacementCycle = items.Average(i => (DateTime.Now - i.addedDate).TotalDays / 365.25)
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting replacement forecast data");
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

// Data Transfer Objects
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

public class RevenueAnalysis
{
    public List<string> Labels { get; set; } = new();
    public List<decimal> RevenueData { get; set; } = new();
    public List<decimal> DamageData { get; set; } = new();
    public decimal TotalRevenue { get; set; }
    public decimal TotalDamage { get; set; }
}

public class ItemsOverTimeData
{
    public List<string> Labels { get; set; } = new();
    public List<int> Data { get; set; } = new();
}

public class DamageAnalysis
{
    public List<string> Labels { get; set; } = new();
    public List<int> DamageCountByMonth { get; set; } = new();
    public int TotalDamaged { get; set; }
}

public class DepartmentAssetData
{
    public List<DepartmentAsset> Departments { get; set; } = new();
    public List<string> Labels { get; set; } = new();
    public List<decimal> Values { get; set; } = new();
}

public class DepartmentAsset
{
    public string DepartmentName { get; set; } = string.Empty;
    public decimal TotalValue { get; set; }
    public int ItemCount { get; set; }
}

public class AgeByTypeData
{
    public List<TypeAgeInfo> TypeAges { get; set; } = new();
    public List<string> Labels { get; set; } = new();
    public List<double> AverageAges { get; set; } = new();
}

public class TypeAgeInfo
{
    public string TypeName { get; set; } = string.Empty;
    public double AverageAgeMonths { get; set; }
    public int ItemCount { get; set; }
}

public class PurchasesVsDisposalsData
{
    public List<string> Labels { get; set; } = new();
    public List<int> Purchases { get; set; } = new();
    public List<int> Disposals { get; set; } = new();
    public int TotalPurchases { get; set; }
    public int TotalDisposals { get; set; }
}

public class AssetValueTrendData
{
    public List<string> Labels { get; set; } = new();
    public List<decimal> Values { get; set; } = new();
    public decimal CurrentValue { get; set; }
    public decimal GrowthRate { get; set; }
}

public class StocktakeProgressData
{
    public List<StocktakeProgress> ActiveStocktakes { get; set; } = new();
    public int TotalStocktakes { get; set; }
    public int CompletedStocktakes { get; set; }
    public int InProgressCount { get; set; }
}

public class StocktakeProgress
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalItems { get; set; }
    public int CheckedItems { get; set; }
    public double ProgressPercentage { get; set; }
    public StockTakeStatus Status { get; set; }
    public int DaysRemaining { get; set; }
    public bool IsOverdue { get; set; }
}

public class DepreciationData
{
    public decimal OriginalValue { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal TotalDepreciation { get; set; }
    public decimal DepreciationRate { get; set; }
    public decimal AnnualDepreciation { get; set; }
}

public class InventoryIssuesData
{
    public int ItemsNotFound { get; set; }
    public int ItemsWithoutLocation { get; set; }
    public int LostItems { get; set; }
    public int DisposedItems { get; set; }
    public int ItemsWithoutReview { get; set; }
    public int ItemsOutOfAssignedLocation { get; set; }
    public int ItemsWithMissingInventoryNumber { get; set; }
    public int ItemsNotCompliantWithPolicy { get; set; }
}

public class ReplacementForecastData
{
    public int ItemsOlderThan5Years { get; set; }
    public decimal EstimatedReplacementCost { get; set; }
    public decimal NextYearForecast { get; set; }
    public decimal Next3YearsForecast { get; set; }
    public Dictionary<string, int> AgeDistribution { get; set; } = new();
    public double AverageReplacementCycle { get; set; }
}