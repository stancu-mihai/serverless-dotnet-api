using System;
using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Main.Persistence;
using Main.Controllers;
using Main.Models;
using System.Text;

namespace Main.Tests
{
    public class UsersControllerTests
    {
        [Fact]
        public async void RegisterNewUser()
        {
            // When checking if the user exists already, return null (doesn't exist)
            var mockRepository = new Mock<IUserRepository>();
            mockRepository.Setup(x => x.GetByUsername("username"))
                .Returns(Task.FromResult<UserItem>(null));

            // When writing any UserItem to db, return this UserItem
            var newUserItem = new UserItem();
            newUserItem.FirstName = "firstName";
            newUserItem.LastName = "lastName";
            newUserItem.Username = "username";
            newUserItem.Role = Role.User; 
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash("password", out passwordHash, out passwordSalt);
            newUserItem.PasswordHash =  passwordHash;
            newUserItem.PasswordSalt = passwordSalt;
            mockRepository.Setup(x => x.Create(It.IsAny<UserItem>()))
                .Returns(Task.FromResult<UserItem>(newUserItem));

            var controller = new UsersController(mockRepository.Object);

            // Act
            UserRegisterRequest request = new UserRegisterRequest();
            request.Username =  "username";
            request.Password =  "password";
            request.FirstName = "firstName";
            request.LastName = "lastName";

            IActionResult actionResult = await controller.Register(request);
            Assert.IsType<OkObjectResult>(actionResult);
            
            OkObjectResult okActionResult = actionResult as OkObjectResult;
            UserRegisterResponse urr = okActionResult.Value as UserRegisterResponse; 
            Assert.Equal(urr.FirstName, newUserItem.FirstName);
            Assert.Equal(urr.LastName, newUserItem.LastName);
            Assert.Equal(urr.Username, newUserItem.Username);
            Assert.Equal(urr.Role, newUserItem.Role);
        }

        [Fact]
        public async void AuthenticateWrongCredentials()
        {
            // Arrange
            var mockRepository = new Mock<IUserRepository>();
            mockRepository.Setup(x => x.GetByUsername("invalidUser"))
                .Returns(Task.FromResult<UserItem>(null));

            var controller = new UsersController(mockRepository.Object);

            // Act
            UserLoginRequest request = new UserLoginRequest();
            request.Username =  "invalidUser";
            request.Password =  "invalidPass";

            IActionResult actionResult = await controller.Authenticate(request);
            Assert.IsType<BadRequestObjectResult>(actionResult);
        }

        [Fact]
        public async void AuthenticateRightCredentials()
        {
            // Arrange
            var mockRepository = new Mock<IUserRepository>();
            Environment.SetEnvironmentVariable("JWT_SECRET", "Some JWT secret for token generation (at least 16chars)");
            byte[] passwordHash = null;
            byte[] passwordSalt = null;
            string password = "validPass";
            CreatePasswordHash(password, out passwordHash, out passwordSalt);
            UserItem item = new UserItem();
            item.Username = "validUser";
            item.FirstName = "validFirstName";
            item.LastName = "validLastName";
            item.Role = Role.User;
            item.PasswordHash = passwordHash;
            item.PasswordSalt = passwordSalt;
            mockRepository.Setup(x => x.GetByUsername("validUser")).ReturnsAsync(item);

            var controller = new UsersController(mockRepository.Object);

            // Act
            UserLoginRequest request = new UserLoginRequest();
            request.Username =  "validUser";
            request.Password =  password;

            IActionResult actionResult = await controller.Authenticate(request);
            Assert.IsType<OkObjectResult>(actionResult);
            OkObjectResult okActionResult = actionResult as OkObjectResult;
            UserLoginResponse ulr = okActionResult.Value as UserLoginResponse; 
            Assert.Equal(ulr.FirstName, item.FirstName);
            Assert.Equal(ulr.LastName, item.LastName);
            Assert.Equal(ulr.Username, item.Username);
            Assert.Equal(ulr.Role, item.Role);
        }

        // Helper methods
        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
    }
}