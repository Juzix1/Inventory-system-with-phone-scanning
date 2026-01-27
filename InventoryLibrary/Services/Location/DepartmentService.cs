using System;
using InventoryLibrary.Data;
using InventoryLibrary.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using InventoryLibrary.Model.Location;

namespace InventoryLibrary.Services.Location;

public class DepartmentService : IDepartmentService
{

    private readonly MyDbContext _context;
    private readonly IInventoryLogger<DepartmentService> _logger;
    public DepartmentService(MyDbContext context, IInventoryLogger<DepartmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Department> CreateDepartmentAsync(Department department)
    {
        try
        {
            if (department == null)
            {
                _logger.LogWarning("Attempted to create a null Department");
                return null;
            }
            await _context.Departments.AddAsync(department);
            _logger.LogInfo($"Created new Department: {department.DepartmentName}");
            return department;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in CreateDepartmentAsync", ex);
            throw;
        }
    }

    public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
    {
        try
        {
            var departments = await _context.Departments.ToListAsync();
            return departments;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in getting departments info", ex);
            throw;
        }
    }

    public async Task<Department?> GetDepartmentByIdAsync(int id)
    {
        try
        {
            var department = await _context.Departments.FindAsync(id);
            return department;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in getting department with {id}", ex);
            throw;
        }
    }

    public async Task DeleteDepartment(Department dep)
    {
        try
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
            _logger.LogWarning($"Deleted Department: {dep.DepartmentName}");
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in DeleteDepartment", ex);
            throw;
        }

    }

    public async Task<IEnumerable<Room>> GetAllRooms()
    {
        try
        {
            var rooms = await _context.Rooms.Include(r => r.Department).ToListAsync();
            return rooms;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in getting rooms info", ex);
            throw;
        }
    }

}
