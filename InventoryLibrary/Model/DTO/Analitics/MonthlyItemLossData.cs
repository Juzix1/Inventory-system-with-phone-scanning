using System;

namespace InventoryLibrary.Model.DTO;

public class MonthlyItemLossData
{
    public List<string> Labels { get; set; } = new();
    public List<int> ItemsLost { get; set; } = new();
    public List<decimal> ValueLost { get; set; } = new();
    public int TotalLost { get; set; }
    public decimal TotalValueLost { get; set; }
    public List<CategoryLossInfo> TopLostCategories { get; set; } = new();
    public decimal AverageLossValue { get; set; }
    public double AverageMonthlyLoss { get; set; }
}
