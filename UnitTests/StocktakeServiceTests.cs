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

namespace Tests
{
    public class StocktakeServiceTests : IDisposable
    {
        private readonly MyDbContext _context;
        private readonly StocktakeService _service;

        public StocktakeServiceTests()
        {
            var options = new DbContextOptionsBuilder<MyDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var logger = new Mock<IInventoryLogger<StocktakeService>>();
            _context = new MyDbContext(options);
            _service = new StocktakeService(_context, logger.Object);
        }

        [Fact]
        public async Task CreateNewStocktake_ValidData_CreatesStocktake()
        {
            // Arrange
            var items = new List<InventoryItem>
            {
                new InventoryItem { Id = 1, itemName = "Item 1" },
                new InventoryItem { Id = 2, itemName = "Item 2" }
            };
            var accounts = new List<Account>
            {
                new Account { Id = 1, Email = "user1@test.com" }
            };

            // Act
            await _service.CreateNewStocktake(
                "Q4 2024 Stocktake",
                "End of year inventory check",
                items,
                accounts,
                DateTime.Now,
                DateTime.Now.AddDays(7)
            );

            // Assert
            Assert.Single(_context.Stocktakes);
            Assert.Equal("Q4 2024 Stocktake", _context.Stocktakes.First().Name);
        }

        [Fact]
        public async Task CreateNewStocktake_EmptyItemsList_ThrowsValidationException()
        {
            // Arrange
            var emptyItems = new List<InventoryItem>();
            var accounts = new List<Account> { new Account { Id = 1 } };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateNewStocktake(
                    "Test", "Desc", emptyItems, accounts,
                    DateTime.Now, DateTime.Now.AddDays(1))
            );
        }

        [Fact]
        public async Task GetAllStocktakesAsync_ReturnsAllStocktakes()
        {
            // Arrange
            _context.Stocktakes.AddRange(
                new Stocktake { Id = 1, Name = "Stocktake 1" },
                new Stocktake { Id = 2, Name = "Stocktake 2" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllStocktakesAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetStocktakeById_ExistingId_ReturnsStocktake()
        {
            // Arrange
            var stocktake = new Stocktake { Id = 1, Name = "Test Stocktake" };
            _context.Stocktakes.Add(stocktake);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetStocktakeById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Stocktake", result.Name);
        }

        [Fact]
        public async Task MarkItemAsChecked_ValidData_MarksItemSuccessfully()
        {
            // Arrange
            var item = new List<InventoryItem>{new InventoryItem { Id = 5, itemName = "Test Item" }};
            var stocktake = new Stocktake
            {
                Id = 1,
                AllItems = 15,
                CheckedItems = new List<StocktakeCheckedItem>(),
                StartDate = DateTime.Now.AddHours(-1), 
                EndDate = DateTime.Now.AddDays(7), 
                CreatedDate = DateTime.Now,
                Status = StockTakeStatus.InProgress,
                ItemsToCheck = item
            };
            _context.Stocktakes.Add(stocktake);
            await _context.SaveChangesAsync();

            // Act
            await _service.MarkItemAsChecked(1, 5, "user@test.com");

            // Assert
            Assert.Single(_context.Stocktakes.First().CheckedItems);
        }

        [Fact]
        public async Task GetStocktakeStatistics_CalculatesCorrectly()
        {

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
                    Id = 2,
                    TypeName = "AGD"
                }
            );
            _context.itemConditions.Add(new ItemCondition
            {
                Id = 1,
                ConditionName = "NEW",
            });
            await _context.SaveChangesAsync();
            // Arrange
            var items = new List<InventoryItem>
            {
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

                    ItemTypeId = 2,
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
                    ItemTypeId = 3,
                    itemPrice = 3000,
                    ItemConditionId = 1,
                    itemName = "Item3",
                    RoomId = 1,
                    Location = room,
                    addedDate = DateTime.Now,
                    warrantyEnd = DateTime.Now.AddYears(1),
                    lastInventoryDate = DateTime.Now
                },
                new InventoryItem
                {
                    ItemTypeId = 4,
                    itemPrice = 3000,
                    ItemConditionId = 1,
                    itemName = "Item4",
                    RoomId = 1,
                    Location = room,
                    addedDate = DateTime.Now,
                    warrantyEnd = DateTime.Now.AddYears(1),
                    lastInventoryDate = DateTime.Now
                }
            };
            var checkedItems = new List<StocktakeCheckedItem>
            {
                new StocktakeCheckedItem { InventoryItemId = 1 },
                new StocktakeCheckedItem { InventoryItemId = 2 },
                
            };
            var stocktake = new Stocktake { Id = 1, ItemsToCheck = items, CheckedItems = checkedItems, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(7), CreatedDate = DateTime.Now, AllItems = items.Count() };
            _context.Stocktakes.Add(stocktake);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetStocktakeStatistics(1);

            // Assert
            Assert.Equal(4, result.TotalItems);
            Assert.Equal(2, result.CheckedItems);
            Assert.Equal(2, result.UncheckedItems);
            Assert.Equal(50, result.ProgressPercentage);
        }

        [Fact]
        public async Task DeleteStocktakeAsync_ExistingStocktake_DeletesSuccessfully()
        {
            // Arrange
            var stocktake = new Stocktake { Id = 1, Name = "Delete Me" };
            _context.Stocktakes.Add(stocktake);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.DeleteStocktakeAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(_context.Stocktakes);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
