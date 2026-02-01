using InventoryLibrary.Data;
using InventoryLibrary.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using InventoryLibrary.Services.Interfaces;
using InventoryLibrary.Model.Inventory;

namespace Tests
{
    public class HistoricalDataServiceTests : IDisposable
    {
        private readonly MyDbContext _context;
        private readonly HistoricalDataService _service;

        public HistoricalDataServiceTests()
        {
            var options = new DbContextOptionsBuilder<MyDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new MyDbContext(options);
            var logger = new Mock<IInventoryLogger<HistoricalDataService>>();
            _service = new HistoricalDataService(_context, logger.Object);
        }

        [Fact]
        public async Task GetAllHistoricalItemsAsync_ReturnsAllItems()
        {
            // Arrange
            _context.HistoricalItems.AddRange(
                new HistoricalItem { Id = 1, itemName = "Deleted Item 1" },
                new HistoricalItem { Id = 2, itemName = "Deleted Item 2" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllHistoricalItemsAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, x => x.itemName == "Deleted Item 1");
        }

        [Fact]
        public async Task CreateHistoricalItemAsync_ValidItem_CreatesRecord()
        {
            // Arrange
            var inventoryItem = new InventoryItem
            {
                Id = 1,
                itemName = "Test Item",
                itemPrice = 1000,
                addedDate = DateTime.Now
            };

            // Act
            var result = await _service.CreateHistoricalItemAsync(inventoryItem);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Item", result.itemName);
        }

        [Fact]
        public async Task DeleteHistoricalItemAsync_ExistingId_DeletesRecord()
        {
            // Arrange
            var historicalItem = new HistoricalItem { Id = 1, itemName = "Test" };
            _context.HistoricalItems.Add(historicalItem);
            await _context.SaveChangesAsync();

            // Act
            await _service.DeleteHistoricalItemAsync(1);

            // Assert
            var allItems = await _context.HistoricalItems.ToListAsync();
            Assert.Empty(allItems);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
