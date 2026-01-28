namespace InventoryLibrary.Model.DTO.Location;

public record class RoomDTO
{
    public int id {get;set;}

    public string roomName {get;set;}
    public int? departmentId {get;set;}

    public string? departmentName {get;set;}

    public string? departmentLocation {get;set;}

}
