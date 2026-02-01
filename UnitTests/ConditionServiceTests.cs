
using InventoryLibrary.Data;
using InventoryLibrary.Services;
using Microsoft.EntityFrameworkCore;
using InventoryLibrary.Model.Inventory;
using Moq;
using InventoryLibrary.Services.Interfaces;

namespace Tests
{
    public class ConditionServiceTests : IDisposable
    {
        private readonly MyDbContext _context;
        private readonly IConditionService _service;

        public ConditionServiceTests()
        {
            // Tworzymy nową bazę InMemory dla każdego testu
            var options = new DbContextOptionsBuilder<MyDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new MyDbContext(options);
            var logger = new Mock<IInventoryLogger<ConditionService>>();
            _service = new ConditionService(_context, logger.Object);
        }

        [Fact]
        public async Task GetAllItemConditions_ReturnsAllConditions()
        {
            // Arrange
            _context.itemConditions.AddRange(
                new ItemCondition { Id = 1, ConditionName = "Excellent" },
                new ItemCondition { Id = 2, ConditionName = "Good" },
                new ItemCondition { Id = 3, ConditionName = "Fair" },
                new ItemCondition { Id = 4, ConditionName = "Poor" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllItemConditions();

            // Assert
 
            Assert.Equal(4, _context.itemConditions.Count());
            Assert.Contains(result.ToList(), c => c.ConditionName == "Excellent");
        }

        [Fact]
        public async Task GetAllItemConditions_EmptyDatabase_ReturnsEmptyList()
        {
            // Arrange
            // nic nie dodajemy do bazy

            // Act
            var result = await _service.GetAllItemConditions();

            // Assert
            Assert.Empty(result);
        }

        public void Dispose()
        {
            // Czyścimy bazę po każdym teście
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
