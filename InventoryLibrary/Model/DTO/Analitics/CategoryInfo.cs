using System;

namespace InventoryLibrary.Model.DTO;

public class CategoryInfo
{
    public string CategoryName { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public decimal TotalValue { get; set; }
    public double Percentage { get; set; }
}
