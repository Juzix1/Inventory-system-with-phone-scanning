using System;

namespace InventoryLibrary.Model.DTO;

public class ItemsByCategoryData
{
    public List<CategoryInfo> Categories { get; set; } = new();
    public List<string> Labels { get; set; } = new();
    public List<int> Values { get; set; } = new();
    public int TotalCategories { get; set; }
}
