namespace InventoryAPI.Model.Inventory
{
    public class Computer : InventoryItem
    {
        public string ModelName { get; set; }
        public string CPU {  get; set; }
        public string RAM { get; set; }
        public string Storage { get; set; }
        public string Graphics { get; set; }
    }
}
