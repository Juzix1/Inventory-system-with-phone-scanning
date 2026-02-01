using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryLibrary.Data;
using InventoryLibrary.Model.Accounts;
using InventoryLibrary.Model.Inventory;
using InventoryLibrary.Model.Location;
using InventoryLibrary.Model.StockTake;
using InventoryLibrary.Services;
using InventoryLibrary.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace InventorySystem.Tests.Services
{
    public class InventoryServiceTests : IDisposable
    {
        private readonly MyDbContext _context;
        private readonly InventoryService _service;
        private readonly IHistoricalDataService _historicalService;

        public InventoryServiceTests()
        {
            var options = new DbContextOptionsBuilder<MyDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new MyDbContext(options);
            var logger = new Mock<IInventoryLogger<HistoricalDataService>>();
            var loggerInv = new Mock<IInventoryLogger<InventoryService>>();

            _historicalService = new HistoricalDataService(_context, logger.Object);
            _service = new InventoryService(_context, _historicalService, loggerInv.Object);
        }

        [Fact]
        public async Task GetAllItemsAsync_ReturnsAllItems()
        {
            // Arrange
            await initData();

            // Act
            var result = await _service.GetAllItemsAsync();

            // Assert
            Assert.Equal(3, result.Count());
            Assert.Contains(result, i => i.itemName == "Item1");
        }

        [Fact]
        public async Task GetItemByIdAsync_ExistingId_ReturnsItem()
        {
            // Arrange
            var item = new InventoryItem
                {
                    ItemTypeId = 2,
                    itemPrice = 1500.32,
                    itemWeight = 1.8,
                    ItemConditionId = 1,
                    itemName = "Test Item",
                    RoomId = 1,
                    Location = _context.Rooms.Find(1),
                    addedDate = DateTime.Now,
                    warrantyEnd = DateTime.Now.AddYears(1),
                    lastInventoryDate = DateTime.Now
                };
            _context.InventoryItems.Add(item);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetItemByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Item", result.itemName);
            Assert.Equal(1500.32, result.itemPrice);
        }

        [Fact]
        public async Task GetItemByIdAsync_NonExistentId_ThrowsNotFoundException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.GetItemByIdAsync(999)
            );
        }

        [Fact]
        public async Task CreateItemAsync_ValidItem_ReturnsCreatedItem()
        {
            // Arrange
            var newItem = new InventoryItem
                {
                    ItemTypeId = 2,
                    itemPrice = 3500.00,
                    itemWeight = 1.8,
                    ItemConditionId = 1,
                    itemName = "New Laptop",
                    RoomId = 1,
                    Location = _context.Rooms.Find(1),
                    addedDate = DateTime.Now,
                    warrantyEnd = DateTime.Now.AddYears(1),
                    lastInventoryDate = DateTime.Now
                };

            // Act
            var result = await _service.CreateItemAsync(newItem);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Laptop", result.itemName);
            var createdInDb = _context.InventoryItems.FirstOrDefault(i => i.Id == result.Id);
            Assert.NotNull(createdInDb);
        }

        [Fact]
        public async Task CreateItemAsync_NullItem_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.CreateItemAsync(null)
            );
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task CreateItemAsync_InvalidName_ThrowsValidationException(string invalidName)
        {   
            await initData();
            // Arrange
            var invalidItem = new InventoryItem
                {
                    ItemTypeId = 2,
                    itemPrice = 3000,
                    ItemConditionId = 1,
                    itemName = invalidName,
                    RoomId = 1,
                    Location = _context.Rooms.Find(1),
                    addedDate = DateTime.Now,
                    warrantyEnd = DateTime.Now.AddYears(1),
                    lastInventoryDate = DateTime.Now
                };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateItemAsync(invalidItem)
            );
        }

        [Fact]
        public async Task UpdateItemAsync_ValidData_ReturnsUpdatedItem()
        {
            // Arrange
            await initData();
            var item = await _context.InventoryItems.FindAsync(1);
            item.itemName = "Updated Laptop";
            item.itemPrice = 4000.00;

            // Act
            var result = await _service.UpdateItemAsync(item);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Laptop", result.itemName);
            Assert.Equal(4000.00, result.itemPrice);
        }

        [Fact]
        public async Task DeleteItemAsync_ExistingItem_DeletesAndCreatesHistoricalRecord()
        {
            // Arrange
            await initData();

            // Act
            var result = await _service.DeleteItemAsync(1);
            // Assert
            Assert.NotNull(result);
            var deletedItem = await _context.InventoryItems.FindAsync(1);
            Assert.Null(deletedItem);
        }

        [Fact]
        public async Task GetItemsByName_ExistingName_ReturnsMatchingItems()
        {
            // Arrange
            await initData();
            await _context.InventoryItems.AddAsync( new InventoryItem
                {

                    ItemTypeId = 1,
                    itemPrice = 2000,
                    ItemConditionId = 1,
                    itemName = "Item1",
                    RoomId = 1,
                    Location = _context.Rooms.Find(1),
                    addedDate = DateTime.Now,
                    warrantyEnd = DateTime.Now.AddYears(1),
                    lastInventoryDate = DateTime.Now
                });
            await _context.SaveChangesAsync();
            // Act
            var result = await _service.GetItemsByName("Item1");

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, i => Assert.Contains("Item1", i.itemName));
        }

        [Fact]
        public async Task GetItemsByName_NoMatches_ReturnsEmptyList()
        {
            // Arrange
            _context.InventoryItems.Add(new InventoryItem { itemName = "Laptop" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetItemsByName("NonExistentItem");

            // Assert
            Assert.Empty(result);
        }

        private async Task initData()
        {
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
                    Id = 2,
                    TypeName = "Furniture"
                }
            );
            _context.itemConditions.Add(new ItemCondition
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
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
