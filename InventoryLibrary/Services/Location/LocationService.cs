using InventoryLibrary.Data;
using InventoryLibrary.Model.Location;
using InventoryLibrary.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryLibrary.Services.Location;

public class LocationService : ILocationService
{
    private readonly MyDbContext _context;

    public LocationService(MyDbContext context)
    {
        _context = context;
    }

    // Departments
    public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
    {
        return await _context.Departments.Include(d => d.Rooms).ToListAsync();
    }

    public async Task<Department?> GetDepartmentByIdAsync(int id)
    {
        return await _context.Departments.Include(d => d.Rooms).FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<Department> CreateDepartmentAsync(Department department)
    {
        if (department is null) throw new ArgumentNullException(nameof(department));
        await _context.Departments.AddAsync(department);
        await _context.SaveChangesAsync();
        return department;
    }

    public async Task<Department> UpdateDepartmentAsync(Department department)
    {
        if (department is null) throw new ArgumentNullException(nameof(department));
        var existing = await _context.Departments.FindAsync(department.Id) ?? throw new KeyNotFoundException("Department not found");
        _context.Entry(existing).CurrentValues.SetValues(department);
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteDepartmentAsync(int id)
    {
        var dep = await _context.Departments.FindAsync(id) ?? throw new KeyNotFoundException("Department not found");
        // unassign rooms
        var rooms = await _context.Rooms.Where(r => r.DepartmentId == id).ToListAsync();
        foreach (var r in rooms)
        {
            r.DepartmentId = null;
        }
        _context.Departments.Remove(dep);
        await _context.SaveChangesAsync();
    }

    // Rooms
    public async Task<IEnumerable<Room>> GetAllRoomsAsync()
    {
        return await _context.Rooms.Include(r => r.Department).ToListAsync();
    }

    public async Task<Room?> GetRoomByIdAsync(int id)
    {
        return await _context.Rooms.Include(r => r.Department).FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Room> CreateRoomAsync(Room room)
    {
        if (room is null) throw new ArgumentNullException(nameof(room));
        await _context.Rooms.AddAsync(room);
        await _context.SaveChangesAsync();
        return room;
    }

    public async Task<Room> UpdateRoomAsync(Room room)
    {
        if (room is null) throw new ArgumentNullException(nameof(room));
        var existing = await _context.Rooms.FindAsync(room.Id) ?? throw new KeyNotFoundException("Room not found");
        _context.Entry(existing).CurrentValues.SetValues(room);
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteRoomAsync(int id)
    {
        var room = await _context.Rooms.FindAsync(id) ?? throw new KeyNotFoundException("Room not found");
        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Room>> GetUnassignedRoomsAsync()
    {
        return await _context.Rooms.Where(r => r.DepartmentId == null).ToListAsync();
    }
}
