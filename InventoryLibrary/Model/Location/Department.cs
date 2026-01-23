using System;
using System.Text.Json.Serialization;

namespace InventoryLibrary.Model.Location;

public class Department
{
    public int Id { get; set; }
    public string DepartmentName { get; set; } = "";
    public string DepartmentLocation { get; set; }

    [JsonIgnore]
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
    
}
