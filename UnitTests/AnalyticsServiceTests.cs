using Moq;
using InventoryLibrary.Services;
using InventoryLibrary.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;
using InventoryLibrary.Model.Inventory;
using InventoryLibrary.Services.Interfaces;

namespace InventorySystem.Tests.Services;

public class AnalyticsServiceTests : IDisposable
{
    private readonly MyDbContext _context;
    private readonly AnalyticsService _service;

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
    _context.InventoryItems.AddRange(
        new InventoryItem 
        { 
            Id = 1, 
            itemPrice = 1000, 
            ItemConditionId = 1,
            itemName = "Item1"  // ✅ Dodane wymagane pole
        },
        new InventoryItem 
        { 
            Id = 2, 
            itemPrice = 2000, 
            ItemConditionId = 2,
            itemName = "Item2"  // ✅ Dodane wymagane pole
        },
        new InventoryItem 
        { 
            Id = 3, 
            itemPrice = 3000, 
            ItemConditionId = 1,
            itemName = "Item3"  // ✅ Dodane wymagane pole
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
    // Używamy obecnego roku, aby dane na pewno się zmieściły w przedziale 12 miesięcy
    var currentYear = DateTime.Now.Year;
    var currentMonth = DateTime.Now.Month;
    
    // Dodajemy itemy w poprzednich miesiącach
    var twoMonthsAgo = DateTime.Now.AddMonths(-2);
    var oneMonthAgo = DateTime.Now.AddMonths(-1);
    
    _context.InventoryItems.AddRange(
        new InventoryItem 
        { 
            addedDate = twoMonthsAgo.AddDays(-5), 
            itemName = "Item1" 
        },
        new InventoryItem 
        { 
            addedDate = twoMonthsAgo.AddDays(-3), 
            itemName = "Item2" 
        },
        new InventoryItem 
        { 
            addedDate = oneMonthAgo.AddDays(-5), 
            itemName = "Item3" 
        }
    );
    await _context.SaveChangesAsync();

    // Act
    var result = await _service.GetMonthlyItemsCreated();

    // Assert
    // Szukamy po formacie "MMM yyyy" używając daty
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
    var electronicsType = new ItemType { Id = 1, TypeName = "Electronics" };
    var furnitureType = new ItemType { Id = 2, TypeName = "Furniture" };
    
    _context.ItemTypes.AddRange(electronicsType, furnitureType);
    await _context.SaveChangesAsync();
    
    _context.InventoryItems.AddRange(
        new InventoryItem 
        { 
            itemName = "Item1", 
            ItemTypeId = 1
        },
        new InventoryItem 
        { 
            itemName = "Item2", 
            ItemTypeId = 1
        },
        new InventoryItem 
        { 
            itemName = "Item3", 
            ItemTypeId = 2
        }
    );
    await _context.SaveChangesAsync();
    
    // Reload context to ensure relationships are loaded
    _context.ChangeTracker.Clear();

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
    
    var itemType = new ItemType { Id = 1, TypeName = "Test Type" };
    _context.ItemTypes.Add(itemType);
    await _context.SaveChangesAsync();
    
    _context.InventoryItems.AddRange(
        new InventoryItem 
        { 
            Id = 1, 
            lastInventoryDate = now.AddDays(-400), // Ponad rok
            itemName = "Item1",
            ItemTypeId = 1
        },
        new InventoryItem 
        { 
            Id = 2, 
            lastInventoryDate = now.AddDays(-30), // 0-3 miesiące
            itemName = "Item2",
            ItemTypeId = 1
        },
        new InventoryItem 
        { 
            Id = 3, 
            lastInventoryDate = now.AddDays(-500), // Ponad rok
            itemName = "Item3",
            ItemTypeId = 1
        }
    );
    await _context.SaveChangesAsync();
    
    // Clear change tracker to ensure fresh query
    _context.ChangeTracker.Clear();

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