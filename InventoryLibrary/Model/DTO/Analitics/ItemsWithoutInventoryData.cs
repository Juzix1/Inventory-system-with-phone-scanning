using System;

namespace InventoryLibrary.Model.DTO;

public class ItemsWithoutInventoryData
{
    public Dictionary<string, int> TimeRanges { get; set; } = new();
    public List<string> Labels { get; set; } = new();
    public List<int> Values { get; set; } = new();
    public List<ItemInventoryStatus> CriticalItems { get; set; } = new();
    public int TotalOverdue { get; set; }
}
