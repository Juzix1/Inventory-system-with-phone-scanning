// using System;

// namespace Tests;

// public class PasswordServiceTests
//     {
//         private readonly PasswordService _service;

//         public PasswordServiceTests()
//         {
//             _service = new PasswordService();
//         }

//         [Fact]
//         public void Hash_ValidPassword_ReturnsHashedString()
//         {
//             // Arrange
//             var password = "SecurePassword123!";

//             // Act
//             var hashedPassword = _service.Hash(password);

//             // Assert
//             hashedPassword.Should().NotBeNullOrEmpty();
//             hashedPassword.Should().NotBe(password);
//         }

//         [Fact]
//         public void Hash_SamePassword_ReturnsDifferentHashes()
//         {
//             // Arrange
//             var password = "TestPassword123";

//             // Act
//             var hash1 = _service.Hash(password);
//             var hash2 = _service.Hash(password);

//             // Assert - bcrypt includes salt, so hashes should be different
//             hash1.Should().NotBe(hash2);
//         }

//         [Fact]
//         public void VerifyPassword_CorrectPassword_ReturnsTrue()
//         {
//             // Arrange
//             var password = "MySecurePassword123!";
//             var hashedPassword = _service.Hash(password);

//             // Act
//             var result = _service.VerifyPassword(hashedPassword, password);

//             // Assert
//             result.Should().BeTrue();
//         }

//         [Fact]
//         public void VerifyPassword_IncorrectPassword_ReturnsFalse()
//         {
//             // Arrange
//             var correctPassword = "CorrectPassword123";
//             var incorrectPassword = "WrongPassword456";
//             var hashedPassword = _service.Hash(correctPassword);

//             // Act
//             var result = _service.VerifyPassword(hashedPassword, incorrectPassword);

//             // Assert
//             result.Should().BeFalse();
//         }

//         [Theory]
//         [InlineData("short")]
//         [InlineData("verylongpasswordthatexceedsreasonablelimits123456789")]
//         [InlineData("P@ssw0rd!")]
//         public void Hash_VariousPasswords_HashesSuccessfully(string password)
//         {
//             // Act
//             var hashedPassword = _service.Hash(password);

//             // Assert
//             hashedPassword.Should().NotBeNullOrEmpty();
//             _service.VerifyPassword(hashedPassword, password).Should().BeTrue();
//         }
//     }