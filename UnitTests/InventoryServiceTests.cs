// using Xunit;
// using Moq;
// using Moq.EntityFrameworkCore;
// using FluentAssertions;
// using Microsoft.EntityFrameworkCore;
// using InventoryLibrary.Services;
// using InventoryLibrary.Services.Interfaces;
// using InventoryLibrary.Model.Accounts;
// using InventoryLibrary.Model.Inventory;
// using InventoryLibrary.Model.StockTake;
// using InventoryLibrary.Model.Location;
// using InventoryLibrary.Model.Data;
// using InventoryLibrary.Data;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;

// namespace InventorySystem.Tests.Services;
// public class InventoryServiceTests
//     {
//         private readonly Mock<MyDbContext> _mockContext;
//         private readonly Mock<IHistoricalDataService> _mockHistoricalService;
//         private readonly InventoryService _service;

//         public InventoryServiceTests()
//         {
//             _mockContext = new Mock<MyDbContext>();
//             _mockHistoricalService = new Mock<IHistoricalDataService>();
//             _service = new InventoryService(_mockContext.Object, _mockHistoricalService.Object);
//         }

//         [Fact]
//         public async Task GetAllItemsAsync_ReturnsAllItems()
//         {
//             // Arrange
//             var items = new List<InventoryItem>
//             {
//                 new InventoryItem { Id = 1, ItemName = "Laptop" },
//                 new InventoryItem { Id = 2, ItemName = "Monitor" },
//                 new InventoryItem { Id = 3, ItemName = "Keyboard" }
//             };
            
//             _mockContext.Setup(x => x.InventoryItems).ReturnsDbSet(items);

//             // Act
//             var result = await _service.GetAllItemsAsync();

//             // Assert
//             result.Should().HaveCount(3);
//             result.Should().Contain(i => i.ItemName == "Laptop");
//         }

//         [Fact]
//         public async Task GetItemByIdAsync_ExistingId_ReturnsItem()
//         {
//             // Arrange
//             var items = new List<InventoryItem>
//             {
//                 new InventoryItem
//                 {
//                     Id = 1,
//                     ItemName = "Test Item",
//                     ItemPrice = 1500.00m,
//                     ItemWeight = 2.5
//                 }
//             };
            
//             _mockContext.Setup(x => x.InventoryItems).ReturnsDbSet(items);

//             // Act
//             var result = await _service.GetItemByIdAsync(1);

//             // Assert
//             result.Should().NotBeNull();
//             result.ItemName.Should().Be("Test Item");
//             result.ItemPrice.Should().Be(1500.00m);
//         }

//         [Fact]
//         public async Task GetItemByIdAsync_NonExistentId_ThrowsNotFoundException()
//         {
//             // Arrange
//             _mockContext.Setup(x => x.InventoryItems).ReturnsDbSet(new List<InventoryItem>());

//             // Act & Assert
//             await Assert.ThrowsAsync<KeyNotFoundException>(
//                 () => _service.GetItemByIdAsync(999)
//             );
//         }

//         [Fact]
//         public async Task CreateItemAsync_ValidItem_ReturnsCreatedItem()
//         {
//             // Arrange
//             var newItem = new InventoryItem
//             {
//                 ItemName = "New Laptop",
//                 ItemDescription = "Dell XPS 15",
//                 ItemPrice = 3500.00m,
//                 ItemWeight = 1.8,
//                 AddedDate = DateTime.Now
//             };
            
//             _mockContext.Setup(x => x.InventoryItems).ReturnsDbSet(new List<InventoryItem>());
//             _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
//                        .ReturnsAsync(1);
//             _mockHistoricalService.Setup(h => h.CreateHistoricalItemAsync(It.IsAny<InventoryItem>()))
//                                  .ReturnsAsync(new HistoricalItem());

//             // Act
//             var result = await _service.CreateItemAsync(newItem);

//             // Assert
//             result.Should().NotBeNull();
//             result.ItemName.Should().Be("New Laptop");
//             _mockHistoricalService.Verify(h => h.CreateHistoricalItemAsync(It.IsAny<InventoryItem>()), Times.Once);
//         }

//         [Fact]
//         public async Task CreateItemAsync_NullItem_ThrowsArgumentNullException()
//         {
//             // Act & Assert
//             await Assert.ThrowsAsync<ArgumentNullException>(
//                 () => _service.CreateItemAsync(null)
//             );
//         }

//         [Theory]
//         [InlineData("")]
//         [InlineData(null)]
//         [InlineData("   ")]
//         public async Task CreateItemAsync_InvalidName_ThrowsValidationException(string invalidName)
//         {
//             // Arrange
//             var invalidItem = new InventoryItem
//             {
//                 ItemName = invalidName,
//                 ItemPrice = 100
//             };

//             // Act & Assert
//             await Assert.ThrowsAsync<ArgumentException>(
//                 () => _service.CreateItemAsync(invalidItem)
//             );
//         }

//         [Fact]
//         public async Task UpdateItemAsync_ValidData_ReturnsUpdatedItem()
//         {
//             // Arrange
//             var items = new List<InventoryItem>
//             {
//                 new InventoryItem { Id = 1, ItemName = "Original Laptop", ItemPrice = 3000.00m }
//             };
            
//             _mockContext.Setup(x => x.InventoryItems).ReturnsDbSet(items);
//             _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
//                        .ReturnsAsync(1);
//             _mockHistoricalService.Setup(h => h.UpdateHistoricalItemAsync(It.IsAny<InventoryItem>()))
//                                  .ReturnsAsync(new HistoricalItem());

//             var updatedItem = new InventoryItem
//             {
//                 Id = 1,
//                 ItemName = "Updated Laptop",
//                 ItemPrice = 4000.00m
//             };

//             // Act
//             var result = await _service.UpdateItemAsync(updatedItem);

//             // Assert
//             result.Should().NotBeNull();
//             _mockHistoricalService.Verify(h => h.UpdateHistoricalItemAsync(It.IsAny<InventoryItem>()), Times.Once);
//         }

//         [Fact]
//         public async Task DeleteItemAsync_ExistingItem_DeletesAndCreatesHistoricalRecord()
//         {
//             // Arrange
//             var items = new List<InventoryItem>
//             {
//                 new InventoryItem { Id = 1, ItemName = "Item to Delete", IsDeleted = false }
//             };
            
//             _mockContext.Setup(x => x.InventoryItems).ReturnsDbSet(items);
//             _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
//                        .ReturnsAsync(1);
//             _mockHistoricalService.Setup(h => h.CreateHistoricalItemAsync(It.IsAny<InventoryItem>()))
//                                  .ReturnsAsync(new HistoricalItem());

//             // Act
//             var result = await _service.DeleteItemAsync(1);

//             // Assert
//             result.Should().NotBeNull();
//             _mockHistoricalService.Verify(h => h.CreateHistoricalItemAsync(It.IsAny<InventoryItem>()), Times.Once);
//         }

//         [Fact]
//         public async Task GetItemsByName_ExistingName_ReturnsMatchingItems()
//         {
//             // Arrange
//             var items = new List<InventoryItem>
//             {
//                 new InventoryItem { Id = 1, ItemName = "Laptop Dell" },
//                 new InventoryItem { Id = 2, ItemName = "Laptop HP" },
//                 new InventoryItem { Id = 3, ItemName = "Monitor" }
//             };
            
//             _mockContext.Setup(x => x.InventoryItems).ReturnsDbSet(items);

//             // Act
//             var result = await _service.GetItemsByName("Laptop");

//             // Assert
//             result.Should().HaveCount(2);
//             result.Should().OnlyContain(i => i.ItemName.Contains("Laptop"));
//         }

//         [Fact]
//         public async Task GetItemsByName_NoMatches_ReturnsEmptyList()
//         {
//             // Arrange
//             var items = new List<InventoryItem>
//             {
//                 new InventoryItem { ItemName = "Laptop" }
//             };
            
//             _mockContext.Setup(x => x.InventoryItems).ReturnsDbSet(items);

//             // Act
//             var result = await _service.GetItemsByName("NonExistentItem");

//             // Assert
//             result.Should().BeEmpty();
//         }
//     }
