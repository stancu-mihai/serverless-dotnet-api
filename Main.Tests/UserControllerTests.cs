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
            mockRepository.Setup(x => x.GetByUsername("validUser"))
                .ReturnsAsync(new UserItem
                    {
                        Username = "validUser",
                        FirstName = "validFirstName",
                        LastName = "validLastName",
                        Role = Role.User,
                        PasswordHash = passwordHash,
                        PasswordSalt = passwordSalt
                    });

            var controller = new UsersController(mockRepository.Object);

            // Act
            UserLoginRequest request = new UserLoginRequest();
            request.Username =  "validUser";
            request.Password =  password;

            IActionResult actionResult = await controller.Authenticate(request);
            Assert.IsType<OkObjectResult>(actionResult);
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