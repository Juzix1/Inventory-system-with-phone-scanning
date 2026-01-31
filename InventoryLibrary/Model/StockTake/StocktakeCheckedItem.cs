using System;

namespace InventoryLibrary.Model.StockTake;

public class StocktakeCheckedItem
    {
        public int Id { get; set; }
        public int StocktakeId { get; set; }
        public int InventoryItemId { get; set; }
        public DateTime CheckedDate { get; set; }
        public Stocktake? Stocktake { get; set; }
        public int? CheckedByUserId { get; set; }
    }