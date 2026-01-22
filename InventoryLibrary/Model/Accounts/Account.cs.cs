using System.Text.Json.Serialization;
using InventoryLibrary.Model.Inventory;

namespace InventoryLibrary.Model.Accounts
{
    public class Account
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        //do zaszyfrowaniah--
        public string? PasswordHash { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public bool IsModerator {get;set;}
        public bool resetPasswordOnNextLogin {get;set;} = false;
        [JsonIgnore]
        public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
    }
}
