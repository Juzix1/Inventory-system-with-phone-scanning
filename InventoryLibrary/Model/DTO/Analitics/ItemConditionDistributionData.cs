using System;

namespace InventoryLibrary.Model.DTO;

public class ItemConditionDistributionData
{
    public List<ConditionInfo> Conditions { get; set; } = new();
    public List<string> Labels { get; set; } = new();
    public List<int> Values { get; set; } = new();
    public int BrokenCount { get; set; }
    public int LostCount { get; set; }
}
