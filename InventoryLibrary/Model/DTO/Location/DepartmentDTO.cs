using System;

namespace InventoryLibrary.Model.DTO.Location;

public record DepartmentDTO
{
    public int id {get;set;}
    public string departmentName{get;set;}

    public string departmentLocation {get;set;}

    public int? roomCount {get;set;}
}

