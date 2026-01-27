using System;

namespace InventoryLibrary.Model.DTO;

public class AssetValueTrendData
{
    public List<string> Labels { get; set; } = new();
    public List<decimal> Values { get; set; } = new();
    public decimal CurrentValue { get; set; }
    public decimal GrowthRate { get; set; }
}
