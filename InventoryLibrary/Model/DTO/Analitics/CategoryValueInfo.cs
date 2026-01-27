using System;

namespace InventoryLibrary.Model.DTO;

public class CategoryValueInfo
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalValue { get; set; }
    public int ItemCount { get; set; }
    public decimal AverageValue { get; set; }
}
