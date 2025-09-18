using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace InventoryLibrary.Model.Inventory {
    public class InventoryItem {
        public int Id { get; set; }

        public string itemName { get; set; }
        public string Barcode { get; set; }
        public string imagePath { get; set;}

        public string? itemDescription { get; set; }

        public string itemCategory { get; set; }
        public string itemCondition { get; set; } //Good condition, Damaged, Lost, Broken, Missing, etc.
        public int quantity { get; set; }

        public double itemWeight { get; set; }
        public double itemPrice { get; set; }

        public DateTime addedDate { get; set; }
        public DateTime warrantyEnd { get; set; }

        public DateTime lastInventoryDate { get; set; }
        //when in charge is null, item is assigned to room
        public string? personInCharge { get; set; }

        public string itemLocation { get; set; } //location in room
    }
}
