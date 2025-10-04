using System;
using InventoryLibrary.Data;
using InventoryLibrary.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using InventoryLibrary.Model.Location;

namespace InventoryLibrary.Services.Location;

public class DepartmentService : IDepartmentService
{

    private readonly MyDbContext _context;
    public DepartmentService(MyDbContext context)
    {
        _context = context;
    }

    public async Task<Department> CreateDepartmentAsync(Department department)
    {
        if (department == null)
        {
            return null;
        }
        await _context.Departments.AddAsync(department);
        return department;
    }

    public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
    {
        var departments = await _context.Departments.ToListAsync();
        return departments;
    }

    public async Task<Department?> GetDepartmentByIdAsync(int id)
    {
        var department = await _context.Departments.FindAsync(id);
        return department;
    }

    public async Task DeleteDepartment(Department dep)
    {
        var items = await _context.InventoryItems
            .Where(i => i.Location.DepartmentId == dep.Id)
            .ToListAsync();

        foreach (var item in items)
        {
            item.RoomId = null;
        }

        if (dep != null)
        {
            _context.Departments.Remove(dep);
        }

        await _context.SaveChangesAsync();

    }

}
