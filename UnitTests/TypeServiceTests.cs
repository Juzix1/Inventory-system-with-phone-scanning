// using System;

// namespace Tests;

// public class TypeServiceTests
//     {
//         private readonly Mock<MyDbContext> _mockContext;
//         private readonly TypeService _service;

//         public TypeServiceTests()
//         {
//             _mockContext = new Mock<MyDbContext>();
//             _service = new TypeService(_mockContext.Object);
//         }

//         [Fact]
//         public void CreateItemType_ValidName_CreatesType()
//         {
//             // Arrange
//             var typeName = "Electronics";
            
//             _mockContext.Setup(x => x.ItemTypes).ReturnsDbSet(new List<ItemType>());
//             _mockContext.Setup(x => x.SaveChanges()).Returns(1);

//             // Act
//             var result = _service.CreateItemType(typeName);

//             // Assert
//             result.Should().NotBeNull();
//             result.TypeName.Should().Be(typeName);
//             _mockContext.Verify(x => x.SaveChanges(), Times.Once);
//         }

//         [Fact]
//         public async Task GetAllTypesAsync_ReturnsAllTypes()
//         {
//             // Arrange
//             var types = new List<ItemType>
//             {
//                 new ItemType { Id = 1, TypeName = "Electronics" },
//                 new ItemType { Id = 2, TypeName = "Furniture" },
//                 new ItemType { Id = 3, TypeName = "Office Supplies" }
//             };
            
//             _mockContext.Setup(x => x.ItemTypes).ReturnsDbSet(types);

//             // Act
//             var result = await _service.GetAllTypesAsync();

//             // Assert
//             result.Should().HaveCount(3);
//             result.Should().Contain(t => t.TypeName == "Electronics");
//         }

//         [Fact]
//         public void DeleteType_ExistingType_DeletesSuccessfully()
//         {
//             // Arrange
//             var type = new ItemType { Id = 1, TypeName = "Old Type" };
//             var types = new List<ItemType> { type };
            
//             _mockContext.Setup(x => x.ItemTypes).ReturnsDbSet(types);
//             _mockContext.Setup(x => x.SaveChanges()).Returns(1);

//             // Act
//             _service.DeleteType(type);

//             // Assert
//             _mockContext.Verify(x => x.SaveChanges(), Times.Once);
//         }

//         [Fact]
//         public void ChangeName_ValidData_UpdatesTypeName()
//         {
//             // Arrange
//             var types = new List<ItemType>
//             {
//                 new ItemType { Id = 1, TypeName = "Electronics" }
//             };
            
//             _mockContext.Setup(x => x.ItemTypes).ReturnsDbSet(types);
//             _mockContext.Setup(x => x.SaveChanges()).Returns(1);

//             // Act
//             _service.ChangeName(1, "Updated Electronics");

//             // Assert
//             _mockContext.Verify(x => x.SaveChanges(), Times.Once);
//         }
//     }
