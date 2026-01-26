using InventoryLibrary.Services;

namespace InventoryLibrary.Services.Interfaces;

public interface IAnalyticsService
{
    // Dashboard Statistics
    Task<DashboardStatistics> GetDashboardStatistics(int? departmentId = null);
    
    // Monthly Items Created
    Task<MonthlyItemsCreatedData> GetMonthlyItemsCreated(int? departmentId = null);
    
    // Items by Category (for PieChart)
    Task<ItemsByCategoryData> GetItemsByCategory(int? departmentId = null);
    
    // Items Without Inventory Review
    Task<ItemsWithoutInventoryData> GetItemsWithoutInventory(int? departmentId = null);
    
    // Monthly Item Loss (from HistoricalData)
    Task<MonthlyItemLossData> GetMonthlyItemLoss(int? departmentId = null);
    
    // Item Condition Distribution
    Task<ItemConditionDistributionData> GetItemConditionDistribution(int? departmentId = null);
    
    // Category Value Analysis
    Task<CategoryValueData> GetCategoryValueAnalysis(int? departmentId = null);
    
    // Asset Value Trend
    Task<AssetValueTrendData> GetAssetValueTrend(int? departmentId = null);
}