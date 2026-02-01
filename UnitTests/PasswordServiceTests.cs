using InventoryLibrary.Services;
using Xunit;

namespace Tests
{
    public class PasswordServiceTests
    {
        private readonly PasswordService _service;

        public PasswordServiceTests()
        {
            _service = new PasswordService();
        }

        [Fact]
        public void Hash_ValidPassword_ReturnsHashedString()
        {
            // Arrange
            var password = "SecurePassword123!";

            // Act
            var hashedPassword = _service.Hash(password);

            // Assert
            Assert.NotEmpty(hashedPassword);
            Assert.NotEqual(password, hashedPassword);
        }

        [Fact]
        public void Hash_SamePassword_ReturnsDifferentHashes()
        {
            // Arrange
            var password = "TestPassword123";

            // Act
            var hash1 = _service.Hash(password);
            var hash2 = _service.Hash(password);

            // Assert
            // bcrypt includes a salt, so hashes should differ
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void VerifyPassword_CorrectPassword_ReturnsTrue()
        {
            // Arrange
            var password = "MySecurePassword123!";
            var hashedPassword = _service.Hash(password);

            // Act
            var result = _service.VerifyPassword(hashedPassword, password);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_IncorrectPassword_ReturnsFalse()
        {
            // Arrange
            var correctPassword = "CorrectPassword123";
            var incorrectPassword = "WrongPassword456";
            var hashedPassword = _service.Hash(correctPassword);

            // Act
            var result = _service.VerifyPassword(hashedPassword, incorrectPassword);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("short")]
        [InlineData("verylongpasswordthatexceedsreasonablelimits123456789")]
        [InlineData("P@ssw0rd!")]
        public void Hash_VariousPasswords_HashesSuccessfully(string password)
        {
            // Act
            var hashedPassword = _service.Hash(password);

            // Assert
            Assert.NotEmpty(hashedPassword);
            Assert.True(_service.VerifyPassword(hashedPassword, password));
        }
    }
}
