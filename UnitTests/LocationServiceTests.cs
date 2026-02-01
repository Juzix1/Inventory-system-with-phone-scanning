
using InventoryLibrary.Data;

using Microsoft.EntityFrameworkCore;

using InventoryLibrary.Model.Location;
using InventoryLibrary.Services.Location;
using Moq;
using InventoryLibrary.Services.Interfaces;

namespace Tests
{
    public class LocationServiceTests : IDisposable
    {
        private readonly MyDbContext _context;
        private readonly LocationService _service;

        public LocationServiceTests()
        {
            var options = new DbContextOptionsBuilder<MyDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new MyDbContext(options);
            var logger = new Mock<IInventoryLogger<LocationService>>();
            _service = new LocationService(_context, logger.Object);
        }

        [Fact]
        public async Task GetAllDepartmentsAsync_ReturnsAllDepartments()
        {
            // Arrange
            _context.Departments.AddRange(
                new Department { Id = 1, DepartmentName = "IT" , DepartmentLocation = "123"},
                new Department { Id = 2, DepartmentName = "HR", DepartmentLocation = "123"}
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllDepartmentsAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, d => d.DepartmentName == "IT");
        }

        [Fact]
        public async Task GetDepartmentByIdAsync_ExistingId_ReturnsDepartment()
        {
            // Arrange
            var dept = new Department
            {
                Id = 1,
                DepartmentName = "Finance",
                DepartmentLocation = "Building A"
            };
            _context.Departments.Add(dept);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetDepartmentByIdAsync(1);

            // Assert
            Assert.NotEmpty(result.ToString());
            Assert.Equal("Finance", result.DepartmentName);

        }

        [Fact]
        public async Task CreateDepartmentAsync_ValidData_CreatesDepartment()
        {
            // Arrange
            var newDept = new Department
            {
                DepartmentName = "Finance",
                DepartmentLocation = "Building A",
                
            };

            // Act
            var result = await _service.CreateDepartmentAsync(newDept);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, await _context.Departments.CountAsync());
        }

        [Fact]
        public async Task DeleteDepartmentAsync_DepartmentHasRooms_ChecksIfRoomsAreUpdated()
        {
            // Arrange
            var dept = new Department { Id = 1, DepartmentName = "IT", DepartmentLocation = "123" };
            var room = new Room { Id = 1, DepartmentId = 1, RoomName = "Room 1" };
            _context.Departments.Add(dept);
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            await _service.DeleteDepartmentAsync(1);
            var rooms = _context.Rooms.FirstOrDefault(i => i.Id == 1);
            var departments = _context.Departments.FindAsync(1);
            // Act & Assert
            Assert.Null(rooms.DepartmentId);
            Assert.Empty(departments.ToString());
        }

        [Fact]
        public async Task GetAllRoomsAsync_ReturnsAllRooms()
        {
            // Arrange
            _context.Rooms.AddRange(
                new Room { Id = 1, RoomName = "Room 101" },
                new Room { Id = 2, RoomName = "Room 102" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllRoomsAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, d => d.RoomName == "Room 101");
            
        }

        [Fact]
        public async Task CreateRoomAsync_ValidData_CreatesRoom()
        {
            // Arrange
            var newRoom = new Room { RoomName = "Conference Room A", DepartmentId = 1 };

            // Act
            var result = await _service.CreateRoomAsync(newRoom);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, await _context.Rooms.CountAsync());
        }

        [Fact]
        public async Task GetUnassignedRoomsAsync_ReturnsRoomsWithoutDepartment()
        {
            // Arrange
            _context.Rooms.AddRange(
                new Room { Id = 1, RoomName = "Room 1", DepartmentId = null },
                new Room { Id = 2, RoomName = "Room 2", DepartmentId = 1 },
                new Room { Id = 3, RoomName = "Room 3", DepartmentId = null }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetUnassignedRoomsAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, r => r.DepartmentId == null);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
