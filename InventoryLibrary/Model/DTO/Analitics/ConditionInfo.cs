using System;

namespace InventoryLibrary.Model.DTO;

public class ConditionInfo
{
    public string ConditionName { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public decimal TotalValue { get; set; }
}
