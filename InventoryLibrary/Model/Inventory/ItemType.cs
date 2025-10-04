using System;
using System.Text.Json.Serialization;

namespace InventoryLibrary.Model.Inventory;

public class ItemType
{
    public int Id { get; set; }

    public string TypeName { get; set; } = "";

    [JsonIgnore]
    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
}
