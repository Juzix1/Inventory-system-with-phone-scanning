using System;
using System.Text.Json.Serialization;

namespace InventoryLibrary.Model.Inventory;

public class ItemCondition
{
    public int Id { get; set; }
    public string ConditionName { get; set; } = "";
    [JsonIgnore]
    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();

}
