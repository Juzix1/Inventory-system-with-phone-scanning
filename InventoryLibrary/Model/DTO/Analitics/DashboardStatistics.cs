using System;

namespace InventoryLibrary.Model.Data;

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
