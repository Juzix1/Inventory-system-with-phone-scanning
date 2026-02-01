// using System;

// namespace Tests;

// public class LocationServiceTests
//     {
//         private readonly Mock<MyDbContext> _mockContext;
//         private readonly LocationService _service;

//         public LocationServiceTests()
//         {
//             _mockContext = new Mock<MyDbContext>();
//             _service = new LocationService(_mockContext.Object);
//         }

//         [Fact]
//         public async Task GetAllDepartmentsAsync_ReturnsAllDepartments()
//         {
//             // Arrange
//             var departments = new List<Department>
//             {
//                 new Department { Id = 1, DepartmentName = "IT" },
//                 new Department { Id = 2, DepartmentName = "HR" }
//             };
            
//             _mockContext.Setup(x => x.Departments).ReturnsDbSet(departments);

//             // Act
//             var result = await _service.GetAllDepartmentsAsync();

//             // Assert
//             result.Should().HaveCount(2);
//             result.Should().Contain(d => d.DepartmentName == "IT");
//         }

//         [Fact]
//         public async Task GetDepartmentByIdAsync_ExistingId_ReturnsDepartment()
//         {
//             // Arrange
//             var departments = new List<Department>
//             {
//                 new Department 
//                 { 
//                     Id = 1, 
//                     DepartmentName = "Finance",
//                     DepartmentLocation = "Building A"
//                 }
//             };
            
//             _mockContext.Setup(x => x.Departments).ReturnsDbSet(departments);

//             // Act
//             var result = await _service.GetDepartmentByIdAsync(1);

//             // Assert
//             result.Should().NotBeNull();
//             result.DepartmentName.Should().Be("Finance");
//         }

//         [Fact]
//         public async Task CreateDepartmentAsync_ValidData_CreatesDepartment()
//         {
//             // Arrange
//             var newDept = new Department
//             {
//                 DepartmentName = "Finance",
//                 DepartmentLocation = "Building A"
//             };

//             _mockContext.Setup(x => x.Departments).ReturnsDbSet(new List<Department>());
//             _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
//                        .ReturnsAsync(1);

//             // Act
//             var result = await _service.CreateDepartmentAsync(newDept);

//             // Assert
//             result.Should().NotBeNull();
//             _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
//         }

//         [Fact]
//         public async Task DeleteDepartmentAsync_DepartmentHasRooms_ThrowsValidationException()
//         {
//             // Arrange
//             var departments = new List<Department>
//             {
//                 new Department { Id = 1, DepartmentName = "IT" }
//             };
//             var rooms = new List<Room>
//             {
//                 new Room { Id = 1, DepartmentId = 1, RoomName = "Room 1" }
//             };
            
//             _mockContext.Setup(x => x.Departments).ReturnsDbSet(departments);
//             _mockContext.Setup(x => x.Rooms).ReturnsDbSet(rooms);

//             // Act & Assert
//             await Assert.ThrowsAsync<InvalidOperationException>(
//                 () => _service.DeleteDepartmentAsync(1)
//             );
//         }

//         [Fact]
//         public async Task GetAllRoomsAsync_ReturnsAllRooms()
//         {
//             // Arrange
//             var rooms = new List<Room>
//             {
//                 new Room { Id = 1, RoomName = "Room 101" },
//                 new Room { Id = 2, RoomName = "Room 102" }
//             };
            
//             _mockContext.Setup(x => x.Rooms).ReturnsDbSet(rooms);

//             // Act
//             var result = await _service.GetAllRoomsAsync();

//             // Assert
//             result.Should().HaveCount(2);
//             result.Should().Contain(r => r.RoomName == "Room 101");
//         }

//         [Fact]
//         public async Task CreateRoomAsync_ValidData_CreatesRoom()
//         {
//             // Arrange
//             var newRoom = new Room
//             {
//                 RoomName = "Conference Room A",
//                 DepartmentId = 1
//             };

//             _mockContext.Setup(x => x.Rooms).ReturnsDbSet(new List<Room>());
//             _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
//                        .ReturnsAsync(1);

//             // Act
//             var result = await _service.CreateRoomAsync(newRoom);

//             // Assert
//             result.Should().NotBeNull();
//             _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
//         }

//         [Fact]
//         public async Task GetUnassignedRoomsAsync_ReturnsRoomsWithoutDepartment()
//         {
//             // Arrange
//             var rooms = new List<Room>
//             {
//                 new Room { Id = 1, RoomName = "Room 1", DepartmentId = null },
//                 new Room { Id = 2, RoomName = "Room 2", DepartmentId = 1 },
//                 new Room { Id = 3, RoomName = "Room 3", DepartmentId = null }
//             };
            
//             _mockContext.Setup(x => x.Rooms).ReturnsDbSet(rooms);

//             // Act
//             var result = await _service.GetUnassignedRoomsAsync();

//             // Assert
//             result.Should().HaveCount(2);
//             result.Should().OnlyContain(r => r.DepartmentId == null);
//         }
//     }