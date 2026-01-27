using System;

namespace InventoryLibrary.Model.DTO;

public class CategoryValueData
{
    public List<CategoryValueInfo> Categories { get; set; } = new();
    public List<string> Labels { get; set; } = new();
    public List<decimal> Values { get; set; } = new();
    public decimal TotalValue { get; set; }
}
