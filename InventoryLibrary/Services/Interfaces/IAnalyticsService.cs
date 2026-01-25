using System;

namespace InventoryLibrary.Services.Interfaces;

public interface IAnalyticsService
{
Task<DashboardStatistics> GetDashboardStatistics(int? departmentId = null);
    Task<RevenueAnalysis> GetRevenueAnalysis(int? departmentId = null);
    Task<ItemsOverTimeData> GetItemsOverTimeData(int? departmentId = null);
    Task<DamageAnalysis> GetDamageByMonthData(int? departmentId = null);
    Task<DepartmentAssetData> GetAssetByDepartmentData();
    Task<AgeByTypeData> GetAverageAgeByTypeData(int? departmentId = null);
    Task<PurchasesVsDisposalsData> GetPurchasesVsDisposalsData(int? departmentId = null);
    Task<AssetValueTrendData> GetAssetValueTrendData(int? departmentId = null);
    Task<StocktakeProgressData> GetStocktakeProgressData();
    Task<DepreciationData> GetDepreciationData(int? departmentId = null);
    Task<InventoryIssuesData> GetInventoryIssuesData(int? departmentId = null);
    Task<ReplacementForecastData> GetReplacementForecastData(int? departmentId = null);
}