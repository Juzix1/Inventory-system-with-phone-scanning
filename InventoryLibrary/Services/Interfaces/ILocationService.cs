using InventoryLibrary.Model.Location;

namespace InventoryLibrary.Services.Interfaces;

public interface ILocationService
{
    // Departments
    Task<IEnumerable<Department>> GetAllDepartmentsAsync();
    Task<Department?> GetDepartmentByIdAsync(int id);
    Task<Department> CreateDepartmentAsync(Department department);
    Task<Department> UpdateDepartmentAsync(Department department);
    Task DeleteDepartmentAsync(int id);

    // Rooms
    Task<IEnumerable<Room>> GetAllRoomsAsync();
    Task<Room?> GetRoomByIdAsync(int id);
    Task<Room> CreateRoomAsync(Room room);
    Task<Room> UpdateRoomAsync(Room room);
    Task DeleteRoomAsync(int id);

    // Helpers
    Task<IEnumerable<Room>> GetUnassignedRoomsAsync();
}
