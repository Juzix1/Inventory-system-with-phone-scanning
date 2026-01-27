using System;

namespace InventoryLibrary.Model.DTO;

public class ItemInventoryStatus
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public DateTime LastInventoryDate { get; set; }
    public int DaysSinceInventory { get; set; }
    public string Category { get; set; } = string.Empty;
}
