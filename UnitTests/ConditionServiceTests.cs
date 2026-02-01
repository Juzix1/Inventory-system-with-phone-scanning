// using System;

// namespace Tests;

// public class ConditionServiceTests
//     {
//         private readonly Mock<MyDbContext> _mockContext;
//         private readonly ConditionService _service;

//         public ConditionServiceTests()
//         {
//             _mockContext = new Mock<MyDbContext>();
//             _service = new ConditionService(_mockContext.Object);
//         }

//         [Fact]
//         public async Task GetAllItemConditions_ReturnsAllConditions()
//         {
//             // Arrange
//             var conditions = new List<ItemCondition>
//             {
//                 new ItemCondition { Id = 1, ConditionName = "Excellent" },
//                 new ItemCondition { Id = 2, ConditionName = "Good" },
//                 new ItemCondition { Id = 3, ConditionName = "Fair" },
//                 new ItemCondition { Id = 4, ConditionName = "Poor" }
//             };
            
//             _mockContext.Setup(x => x.ItemConditions).ReturnsDbSet(conditions);

//             // Act
//             var result = await _service.GetAllItemConditions();

//             // Assert
//             result.Should().HaveCount(4);
//             result.Should().Contain(c => c.ConditionName == "Excellent");
//         }

//         [Fact]
//         public async Task GetAllItemConditions_EmptyDatabase_ReturnsEmptyList()
//         {
//             // Arrange
//             _mockContext.Setup(x => x.ItemConditions).ReturnsDbSet(new List<ItemCondition>());

//             // Act
//             var result = await _service.GetAllItemConditions();

//             // Assert
//             result.Should().BeEmpty();
//         }
//     }