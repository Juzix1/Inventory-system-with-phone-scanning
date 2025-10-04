using System;
using InventoryLibrary.Model.Location;

namespace InventoryLibrary.Services.Interfaces;

public interface IDepartmentService
{
    Task<IEnumerable<Department>> GetAllDepartmentsAsync();

    Task<Department?> GetDepartmentByIdAsync(int id);
    Task DeleteDepartment(Department dep);
    Task<Department> CreateDepartmentAsync(Department department);
}
