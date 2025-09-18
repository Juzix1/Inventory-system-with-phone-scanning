using System;

namespace InventoryWeb.Models;

public class InventoryItemGroup
    {
        public string ItemName { get; set; }
        public string ItemCategory { get; set; }
        public string ImagePath { get; set; }
        public DateTime WarrantyEnd { get; set; }
        public int InStock { get; set; }
    }