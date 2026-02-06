using InventoryLibrary.Model.Data;
using InventoryLibrary.Model.DTO;
using InventoryLibrary.Services;

namespace InventoryLibrary.Services.Interfaces;

public interface IAnalyticsService
{
    Task<DashboardStatistics> GetDashboardStatistics(int? departmentId = null);
    Task<MonthlyItemsCreatedData> GetMonthlyItemsCreated(int? departmentId = null);
    Task<ItemsByCategoryData> GetItemsByCategory(int? departmentId = null);
    Task<ItemsWithoutInventoryData> GetItemsWithoutInventory(int? departmentId = null);
    Task<MonthlyItemLossData> GetMonthlyItemLoss(int? departmentId = null);
    Task<ItemConditionDistributionData> GetItemConditionDistribution(int? departmentId = null);
    Task<CategoryValueData> GetCategoryValueAnalysis(int? departmentId = null);
    Task<AssetValueTrendData> GetAssetValueTrend(int? departmentId = null);
}