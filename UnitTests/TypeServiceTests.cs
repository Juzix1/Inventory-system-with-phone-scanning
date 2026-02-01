using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InventoryLibrary.Data;
using InventoryLibrary.Model.Inventory;
using InventoryLibrary.Services;
using InventoryLibrary.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Tests
{
    public class TypeServiceTests : IDisposable
    {
        private readonly MyDbContext _context;
        private readonly TypeService _service;

        public TypeServiceTests()
        {
            var options = new DbContextOptionsBuilder<MyDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var logger = new Mock<IInventoryLogger<TypeService>>();
            _context = new MyDbContext(options);
            _service = new TypeService(_context, logger.Object);
        }

        [Fact]
        public void CreateItemType_ValidName_CreatesType()
        {
            // Arrange
            var typeName = "Electronics";

            // Act
            var result = _service.CreateItemType(typeName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(typeName, result.TypeName);
        }

        [Fact]
        public async Task GetAllTypesAsync_ReturnsAllTypes()
        {
            // Arrange
            _context.ItemTypes.AddRange(
                new ItemType { Id = 1, TypeName = "Electronics" },
                new ItemType { Id = 2, TypeName = "Furniture" },
                new ItemType { Id = 3, TypeName = "Office Supplies" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllTypesAsync();

            // Assert
            Assert.Equal(3, result.Count());
            Assert.Contains(result, i => i.TypeName == "Electronics");
        }

        [Fact]
        public void DeleteType_ExistingType_DeletesSuccessfully()
        {
            // Arrange
            var type = new ItemType { Id = 1, TypeName = "Old Type" };
            _context.ItemTypes.Add(type);
            _context.SaveChanges();

            // Act
            _service.DeleteType(type);

            // Assert
            Assert.Empty(_context.ItemTypes.Where(i => i.Id == 1));
        }

        [Fact]
        public void ChangeName_ValidData_UpdatesTypeName()
        {
            // Arrange
            var type = new ItemType { Id = 1, TypeName = "Electronics" };
            _context.ItemTypes.Add(type);
            _context.SaveChanges();

            // Act
            _service.ChangeName(1, "Updated Electronics");

            // Assert
            var updatedType = _context.ItemTypes.Find(1);
            Assert.Equal("Updated Electronics", updatedType.TypeName);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
