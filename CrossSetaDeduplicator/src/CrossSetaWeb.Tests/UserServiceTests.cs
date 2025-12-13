using System;
using CrossSetaWeb.DataAccess;
using CrossSetaWeb.Models;
using CrossSetaWeb.Services;
using Moq;
using Xunit;

namespace CrossSetaWeb.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IDatabaseHelper> _mockDbHelper;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockDbHelper = new Mock<IDatabaseHelper>();
            _userService = new UserService(_mockDbHelper.Object);
        }

        [Fact]
        public void RegisterUser_ThrowsArgumentException_WhenUsernameIsEmpty()
        {
            // Arrange
            var user = new UserModel { UserName = "" };
            var password = "password123";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _userService.RegisterUser(user, password));
        }

        [Fact]
        public void RegisterUser_ThrowsArgumentException_WhenPasswordIsEmpty()
        {
            // Arrange
            var user = new UserModel { UserName = "testuser" };
            var password = "";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _userService.RegisterUser(user, password));
        }

        [Fact]
        public void RegisterUser_CallsInsertUser_WhenDataIsValid()
        {
            // Arrange
            var user = new UserModel { UserName = "testuser" };
            var password = "password123";

            // Act
            _userService.RegisterUser(user, password);

            // Assert
            _mockDbHelper.Verify(d => d.InsertUser(It.Is<UserModel>(u => 
                u.UserName == "testuser" && 
                !string.IsNullOrEmpty(u.PasswordHash) &&
                u.PasswordHash != password
            )), Times.Once);
        }
    }
}
