using System;

namespace InventoryLibrary.Model.DTO;

public class CategoryLossInfo
{
    public string CategoryName { get; set; } = string.Empty;
    public int ItemsLost { get; set; }
    public decimal TotalValueLost { get; set; }
}
