using InventoryLibrary.Model.Accounts;
using InventoryLibrary.Model.Location;
using InventoryLibrary.Model.StockTake;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace InventoryLibrary.Model.Inventory {
    public class InventoryItem
    {
        public int Id { get; set; }

        public string itemName { get; set; }
        public string Barcode { get; set; }
        public string? imagePath { get; set; } = "";

        public string? itemDescription { get; set; }
        public int ItemTypeId { get; set; }
        public ItemType? ItemType { get; set; }
        public int ItemConditionId { get; set; }
        public ItemCondition? ItemCondition { get; set; }
        public double itemWeight { get; set; }
        public double itemPrice { get; set; }

        public DateTime addedDate { get; set; }
        public DateTime warrantyEnd { get; set; }

        public DateTime lastInventoryDate { get; set; }
        //when in charge is null, item is assigned to room
        public int? PersonInChargeId { get; set; }
        public Account? personInCharge { get; set; }
        public int? RoomId { get; set; }
        public Room? Location { get; set; }
        public int? StocktakeId {get;set;}
        public Stocktake? Stocktake {get;set;}
    }
}
