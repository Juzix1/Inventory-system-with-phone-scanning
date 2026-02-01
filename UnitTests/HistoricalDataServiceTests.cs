// using System;

// namespace Tests;

// public class HistoricalDataServiceTests
//     {
//         private readonly Mock<MyDbContext> _mockContext;
//         private readonly HistoricalDataService _service;

//         public HistoricalDataServiceTests()
//         {
//             _mockContext = new Mock<MyDbContext>();
//             _service = new HistoricalDataService(_mockContext.Object);
//         }

//         [Fact]
//         public async Task GetAllHistoricalItemsAsync_ReturnsAllItems()
//         {
//             // Arrange
//             var historicalItems = new List<HistoricalItem>
//             {
//                 new HistoricalItem { Id = 1, ItemName = "Deleted Item 1" },
//                 new HistoricalItem { Id = 2, ItemName = "Deleted Item 2" }
//             };
            
//             _mockContext.Setup(x => x.HistoricalItems).ReturnsDbSet(historicalItems);

//             // Act
//             var result = await _service.GetAllHistoricalItemsAsync();

//             // Assert
//             result.Should().HaveCount(2);
//         }

//         [Fact]
//         public async Task CreateHistoricalItemAsync_ValidItem_CreatesRecord()
//         {
//             // Arrange
//             var inventoryItem = new InventoryItem
//             {
//                 Id = 1,
//                 ItemName = "Test Item",
//                 ItemPrice = 1000,
//                 AddedDate = DateTime.Now
//             };

//             _mockContext.Setup(x => x.HistoricalItems).ReturnsDbSet(new List<HistoricalItem>());
//             _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
//                        .ReturnsAsync(1);

//             // Act
//             var result = await _service.CreateHistoricalItemAsync(inventoryItem);

//             // Assert
//             result.Should().NotBeNull();
//             result.ItemName.Should().Be("Test Item");
//             _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
//         }

//         [Fact]
//         public async Task DeleteHistoricalItemAsync_ExistingId_DeletesRecord()
//         {
//             // Arrange
//             var historicalItems = new List<HistoricalItem>
//             {
//                 new HistoricalItem { Id = 1, ItemName = "Test" }
//             };
            
//             _mockContext.Setup(x => x.HistoricalItems).ReturnsDbSet(historicalItems);
//             _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
//                        .ReturnsAsync(1);

//             // Act
//             await _service.DeleteHistoricalItemAsync(1);

//             // Assert
//             _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
//         }
//     }