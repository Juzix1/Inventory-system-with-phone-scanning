using InventoryLibrary.Data;
using InventoryLibrary.Model.Location;
using InventoryLibrary.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryLibrary.Services.Location;

public class LocationService : ILocationService
{
    private readonly MyDbContext _context;
    private readonly IInventoryLogger<LocationService> _logger;
    public LocationService(MyDbContext context, IInventoryLogger<LocationService> logger)
    {
        _context = context;
        _logger = logger;
    }

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
        try
        {
            if (department is null) throw new ArgumentNullException(nameof(department));
            await _context.Departments.AddAsync(department);
            await _context.SaveChangesAsync();
            _logger.LogInfo($"Created new Department: {department.DepartmentName}");
            return department;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating department", ex);
            throw new Exception("Error creating department", ex);
        }
    }

    public async Task<Department> UpdateDepartmentAsync(Department department)
    {
        try
        {
            if (department is null) throw new ArgumentNullException(nameof(department));
            var existing = await _context.Departments.FindAsync(department.Id) ?? throw new KeyNotFoundException("Department not found");
            _context.Entry(existing).CurrentValues.SetValues(department);
            await _context.SaveChangesAsync();
            _logger.LogInfo($"Updated Department: {department.DepartmentName}");
            return existing;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error updating department", ex);
            throw new Exception("Error updating department", ex);
        }
    }

    public async Task DeleteDepartmentAsync(int id)
    {
        try
        {
            var dep = await _context.Departments.FindAsync(id) ?? throw new KeyNotFoundException("Department not found");
            var rooms = await _context.Rooms.Where(r => r.DepartmentId == id).ToListAsync();
            foreach (var r in rooms)
            {
                r.DepartmentId = null;
            }
            _context.Departments.Remove(dep);
            _logger.LogWarning($"Deleted Department: {dep.DepartmentName}");
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error deleting department", ex);
            throw new Exception("Error deleting department", ex);
        }
    }

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
        try
        {
            if (room is null) throw new ArgumentNullException(nameof(room));
            await _context.Rooms.AddAsync(room);
            await _context.SaveChangesAsync();
            _logger.LogInfo($"Created new Room: {room.RoomName}");
            return room;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating room", ex);
            throw new Exception("Error creating room", ex);
        }
    }

    public async Task<Room> UpdateRoomAsync(Room room)
    {
        try
        {
            if (room is null) throw new ArgumentNullException(nameof(room));
            var existing = await _context.Rooms.FindAsync(room.Id) ?? throw new KeyNotFoundException("Room not found");
            _context.Entry(existing).CurrentValues.SetValues(room);
            await _context.SaveChangesAsync();
            _logger.LogInfo($"Updated Room: {room.RoomName}");
            return existing;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error updating room", ex);
            throw new Exception("Error updating room", ex);
        }
    }

    public async Task DeleteRoomAsync(int id)
    {
        try
        {
            var room = await _context.Rooms.FindAsync(id) ?? throw new KeyNotFoundException("Room not found");
            _context.Rooms.Remove(room);
            _logger.LogWarning($"Deleted Room: {room.RoomName}");
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error deleting room", ex);
            throw new Exception("Error deleting room", ex);
        }
    }

    public async Task<IEnumerable<Room>> GetUnassignedRoomsAsync()
    {
        return await _context.Rooms.Where(r => r.DepartmentId == null).ToListAsync();
    }
}
