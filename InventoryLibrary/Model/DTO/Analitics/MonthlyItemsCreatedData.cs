using System;

namespace InventoryLibrary.Model.DTO;

public class MonthlyItemsCreatedData
{
    public List<string> Labels { get; set; } = new();
    public List<int> ItemsCreated { get; set; } = new();
    public int TotalCreated { get; set; }
    public double AveragePerMonth { get; set; }
}
