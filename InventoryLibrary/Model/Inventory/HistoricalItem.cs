using System;

namespace InventoryLibrary.Model.Inventory;

public class HistoricalItem
{
    public int Id { get; set; }
    public int OriginalItemId { get; set; }
    public string itemName { get; set; } = string.Empty;
    public string? itemDescription { get; set; } = string.Empty;
    public int ItemTypeId { get; set; }
    public ItemType? ItemType { get; set; }
    public double itemWeight { get; set; }
    public double itemPrice { get; set; }
    public DateTime addedDate { get; set; }
    public DateTime archivedDate { get; set; }

}
