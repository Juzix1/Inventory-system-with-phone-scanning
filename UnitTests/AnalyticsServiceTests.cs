using Moq;
using InventoryLibrary.Services;
using InventoryLibrary.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;
using InventoryLibrary.Model.Inventory;
using InventoryLibrary.Services.Interfaces;
using InventoryLibrary.Model.Location;

namespace InventorySystem.Tests.Services;

public class AnalyticsServiceTests : IDisposable
{
    private readonly MyDbContext _context;
    private readonly IAnalyticsService _service;

    public AnalyticsServiceTests()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MyDbContext(options);
        var logger = new Mock<IInventoryLogger<AnalyticsService>>();
        _service = new AnalyticsService(_context, logger.Object);
    }

[Fact]
public async Task GetDashboardStatistics_NoFilter_ReturnsGlobalStatistics()
{
    // Arrange
    var department = new Department { Id = 1, DepartmentName = "Test Dept", DepartmentLocation = "Building A" };
    var room = new Room { Id = 1, RoomName = "Test Room", DepartmentId = 1 };
    _context.Departments.Add(department);
    _context.Rooms.Add(room);

    _context.ItemTypes.AddRange(
        new ItemType
        {
            Id = 1,
            TypeName = "Electrics"
        },
        new ItemType
        {
            Id=2,
            TypeName = "AGD"
        }
    );
    _context.itemConditions.Add( new ItemCondition
    {
        Id = 1,
        ConditionName = "NEW",
    });
    
    _context.InventoryItems.AddRange(
        new InventoryItem 
        { 
            ItemTypeId = 1,
            itemPrice = 1000, 
            ItemConditionId = 1,
            itemName = "Item1",
            RoomId = 1,
            addedDate = DateTime.Now,
            Location = room,
            warrantyEnd = DateTime.Now.AddYears(1),
            lastInventoryDate = DateTime.Now
        },
        new InventoryItem 
        { 
            
            ItemTypeId = 1,
            itemPrice = 2000, 
            ItemConditionId = 1,
            itemName = "Item2",
            Location = room,
            RoomId = 1,
            addedDate = DateTime.Now,
            warrantyEnd = DateTime.Now.AddYears(1),
            lastInventoryDate = DateTime.Now
        },
        new InventoryItem 
        { 
            ItemTypeId = 2,
            itemPrice = 3000, 
            ItemConditionId = 1,
            itemName = "Item3",
            RoomId = 1,
            Location = room,
            addedDate = DateTime.Now,
            warrantyEnd = DateTime.Now.AddYears(1),
            lastInventoryDate = DateTime.Now
        }
    );
    await _context.SaveChangesAsync();
    
    // Act
    var result = await _service.GetDashboardStatistics();

    // Assert
    Assert.Equal(3, result.TotalItems);
    Assert.Equal(6000, result.TotalValue);
}

[Fact]
public async Task GetMonthlyItemsCreated_ReturnsCorrectMonthlyData()
{
    // Arrange
     var department = new Department { Id = 1, DepartmentName = "Test Dept", DepartmentLocation = "Building A" };
    var room = new Room { Id = 1, RoomName = "Test Room", DepartmentId = 1 };
    _context.Departments.Add(department);
    _context.Rooms.Add(room);

    _context.ItemTypes.AddRange(
        new ItemType
        {
            Id = 1,
            TypeName = "Electrics"
        },
        new ItemType
        {
            Id=2,
            TypeName = "AGD"
        }
    );
    _context.itemConditions.Add( new ItemCondition
    {
        Id = 1,
        ConditionName = "NEW",
    });
    
    _context.InventoryItems.AddRange(
        new InventoryItem 
        { 
            ItemTypeId = 1,
            itemPrice = 1000, 
            ItemConditionId = 1,
            itemName = "Item1",
            RoomId = 1,
            addedDate = DateTime.Now.AddMonths(-2),
            Location = room,
            warrantyEnd = DateTime.Now.AddYears(1),
            lastInventoryDate = DateTime.Now
        },
        new InventoryItem 
        { 
            
            ItemTypeId = 1,
            itemPrice = 2000, 
            ItemConditionId = 1,
            itemName = "Item2",
            Location = room,
            RoomId = 1,
            addedDate = DateTime.Now.AddMonths(-2),
            warrantyEnd = DateTime.Now.AddYears(1),
            lastInventoryDate = DateTime.Now
        },
        new InventoryItem 
        { 
            ItemTypeId = 2,
            itemPrice = 3000, 
            ItemConditionId = 1,
            itemName = "Item3",
            RoomId = 1,
            Location = room,
            addedDate = DateTime.Now.AddMonths(-1),
            warrantyEnd = DateTime.Now.AddYears(1),
            lastInventoryDate = DateTime.Now
        }
    );
    await _context.SaveChangesAsync();
    var twoMonthsAgo = DateTime.Now.AddMonths(-2);
    var oneMonthAgo = DateTime.Now.AddMonths(-1);

    // Act
    var result = await _service.GetMonthlyItemsCreated();

    // Assert
    var twoMonthsAgoLabel = twoMonthsAgo.ToString("MMM yyyy");
    var oneMonthAgoLabel = oneMonthAgo.ToString("MMM yyyy");
    
    var twoMonthsIndex = result.Labels.IndexOf(twoMonthsAgoLabel);
    var oneMonthIndex = result.Labels.IndexOf(oneMonthAgoLabel);
    
    Assert.NotEqual(-1, twoMonthsIndex);
    Assert.NotEqual(-1, oneMonthIndex);
    
    Assert.Equal(2, result.ItemsCreated[twoMonthsIndex]);
    Assert.Equal(1, result.ItemsCreated[oneMonthIndex]);
    Assert.Equal(3, result.TotalCreated);
}

[Fact]
public async Task GetItemsByCategory_GroupsItemsCorrectly()
{
    // Arrange
    var department = new Department { Id = 1, DepartmentName = "Test Dept", DepartmentLocation = "Building A" };
    var room = new Room { Id = 1, RoomName = "Test Room", DepartmentId = 1 };
    _context.Departments.Add(department);
    _context.Rooms.Add(room);

    _context.ItemTypes.AddRange(
        new ItemType
        {
            Id = 1,
            TypeName = "Electronics"
        },
        new ItemType
        {
            Id=2,
            TypeName = "Furniture"
        }
    );
    _context.itemConditions.Add( new ItemCondition
    {
        Id = 1,
        ConditionName = "NEW",
    });
    
    _context.InventoryItems.AddRange(
        new InventoryItem 
        { 
            ItemTypeId = 1,
            itemPrice = 1000, 
            ItemConditionId = 1,
            itemName = "Item1",
            RoomId = 1,
            addedDate = DateTime.Now,
            Location = room,
            warrantyEnd = DateTime.Now.AddYears(1),
            lastInventoryDate = DateTime.Now
        },
        new InventoryItem 
        { 
            
            ItemTypeId = 1,
            itemPrice = 2000, 
            ItemConditionId = 1,
            itemName = "Item2",
            Location = room,
            RoomId = 1,
            addedDate = DateTime.Now,
            warrantyEnd = DateTime.Now.AddYears(1),
            lastInventoryDate = DateTime.Now
        },
        new InventoryItem 
        { 
            ItemTypeId = 2,
            itemPrice = 3000, 
            ItemConditionId = 1,
            itemName = "Item3",
            RoomId = 1,
            Location = room,
            addedDate = DateTime.Now,
            warrantyEnd = DateTime.Now.AddYears(1),
            lastInventoryDate = DateTime.Now
        }
    );
    await _context.SaveChangesAsync();

    // Act
    var result = await _service.GetItemsByCategory();

    // Assert
    var categories = result.Categories.ToList();
    
    Assert.True(categories.Count >= 2, $"Expected at least 2 categories, got {categories.Count}");
    
    var electronics = categories.FirstOrDefault(c => c.CategoryName == "Electronics");
    var furniture = categories.FirstOrDefault(c => c.CategoryName == "Furniture");
    
    Assert.NotNull(electronics);
    Assert.NotNull(furniture);
    Assert.Equal(2, electronics.ItemCount);
    Assert.Equal(1, furniture.ItemCount);
}

[Fact]
public async Task GetItemsWithoutInventory_ReturnsItemsNeverChecked()
{
    // Arrange
    var now = DateTime.Now;
    
    var department = new Department { Id = 1, DepartmentName = "Test Dept", DepartmentLocation = "Building A" };
    var room = new Room { Id = 1, RoomName = "Test Room", DepartmentId = 1 };
    _context.Departments.Add(department);
    _context.Rooms.Add(room);

    _context.ItemTypes.AddRange(
        new ItemType
        {
            Id = 1,
            TypeName = "Electrics"
        },
        new ItemType
        {
            Id=2,
            TypeName = "AGD"
        }
    );
    _context.itemConditions.Add( new ItemCondition
    {
        Id = 1,
        ConditionName = "NEW",
    });
    
    _context.InventoryItems.AddRange(
        new InventoryItem 
        { 
            ItemTypeId = 1,
            itemPrice = 1000, 
            ItemConditionId = 1,
            itemName = "Item1",
            RoomId = 1,
            addedDate = DateTime.Now,
            Location = room,
            warrantyEnd = DateTime.Now.AddYears(1),
            lastInventoryDate = DateTime.Now.AddDays(-365)
        },
        new InventoryItem 
        { 
            
            ItemTypeId = 1,
            itemPrice = 2000, 
            ItemConditionId = 1,
            itemName = "Item2",
            Location = room,
            RoomId = 1,
            addedDate = DateTime.Now,
            warrantyEnd = DateTime.Now.AddYears(1),
            lastInventoryDate = DateTime.Now.AddDays(-365)
        },
        new InventoryItem 
        { 
            ItemTypeId = 2,
            itemPrice = 3000, 
            ItemConditionId = 1,
            itemName = "Item3",
            RoomId = 1,
            Location = room,
            addedDate = DateTime.Now,
            warrantyEnd = DateTime.Now.AddYears(1),
            lastInventoryDate = DateTime.Now.AddDays(-455)
        }
    );
    await _context.SaveChangesAsync();

    // Act
    var result = await _service.GetItemsWithoutInventory();

    // Assert
    Assert.True(result.TotalOverdue >= 2, $"Expected at least 2 overdue items, got {result.TotalOverdue}");
    Assert.True(result.CriticalItems.Count >= 2, $"Expected at least 2 critical items, got {result.CriticalItems.Count}");
    
    var criticalItemIds = result.CriticalItems.Select(i => i.ItemId).ToList();
    Assert.Contains(1, criticalItemIds);
    Assert.Contains(3, criticalItemIds);
}

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}