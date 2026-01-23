using System;

namespace InventoryLibrary.Model.Location;

public class Room
{
    public int Id { get; set; }
    public string RoomName { get; set; } = "";
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }
}
