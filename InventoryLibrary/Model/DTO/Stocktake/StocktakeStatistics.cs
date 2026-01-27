using System;

namespace InventoryLibrary.Model.DTO.Stocktake;

public class StocktakeStatistics
{
    public int TotalItems { get; set; }
    public int CheckedItems { get; set; }
    public int UncheckedItems { get; set; }
    public double ProgressPercentage { get; set; }
    public int DaysRemaining { get; set; }
    public bool IsOverdue { get; set; }
    public double AverageItemsPerDay { get; set; }
}
