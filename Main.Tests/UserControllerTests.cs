using System;
using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Main.Persistence;
using Main.Controllers;
using Main.Models;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Collections.Generic;
using System.Security.Principal;
using System.Security.Claims;

namespace Main.Tests
{
    public class UsersControllerTests
    {
        [Fact]
        public async void RegisterNewUser()
        {
            // When checking if the user exists already, return null (doesn't exist)
            var mockRepository = new Mock<IUserRepository>();
            mockRepository.Setup(x => x.GetByEmail("email"))
                .Returns(Task.FromResult<UserItem>(null));

            // When writing any UserItem to db, return this UserItem
            var newUserItem = new UserItem();
            newUserItem.FirstName = "firstName";
            newUserItem.LastName = "lastName";
            newUserItem.Email = "email";
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
            request.Email =  "email";
            request.Password =  "password";
            request.FirstName = "firstName";
            request.LastName = "lastName";

            IActionResult actionResult = await controller.Register(request);
            Assert.IsType<OkObjectResult>(actionResult);
            
            OkObjectResult okActionResult = actionResult as OkObjectResult;
            UserRegisterResponse urr = okActionResult.Value as UserRegisterResponse; 
            Assert.Equal(urr.FirstName, newUserItem.FirstName);
            Assert.Equal(urr.LastName, newUserItem.LastName);
            Assert.Equal(urr.Email, newUserItem.Email);
            Assert.Equal(urr.Role, newUserItem.Role);
        }

        [Fact]
        public async void AuthenticateWrongCredentials()
        {
            // Arrange
            var mockRepository = new Mock<IUserRepository>();
            mockRepository.Setup(x => x.GetByEmail("invalidUserEmail"))
                .Returns(Task.FromResult<UserItem>(null));

            var controller = new UsersController(mockRepository.Object);

            // Act
            UserLoginRequest request = new UserLoginRequest();
            request.Email =  "invalidUser";
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
            item.Email = "validEmail";
            item.FirstName = "validFirstName";
            item.LastName = "validLastName";
            item.Role = Role.User;
            item.PasswordHash = passwordHash;
            item.PasswordSalt = passwordSalt;
            mockRepository.Setup(x => x.GetByEmail("validUserEmail")).ReturnsAsync(item);

            var controller = new UsersController(mockRepository.Object);

            // Act
            UserLoginRequest request = new UserLoginRequest();
            request.Email =  "validEmail";
            request.Password =  password;

            IActionResult actionResult = await controller.Authenticate(request);
            Assert.IsType<OkObjectResult>(actionResult);
            OkObjectResult okActionResult = actionResult as OkObjectResult;
            UserLoginResponse ulr = okActionResult.Value as UserLoginResponse; 
            Assert.Equal(ulr.FirstName, item.FirstName);
            Assert.Equal(ulr.LastName, item.LastName);
            Assert.Equal(ulr.Email, item.Email);
            Assert.Equal(ulr.Role, item.Role);
        }

        [Fact]
        public async void GetAllRightCredentials()
        {
            // Arrange
            var mockRepository = new Mock<IUserRepository>();
            Environment.SetEnvironmentVariable("JWT_SECRET", "Some JWT secret for token generation (at least 16chars)");
            byte[] passwordHash = null;
            byte[] passwordSalt = null;
            string password = "validPass";
            CreatePasswordHash(password, out passwordHash, out passwordSalt);
            UserItem userItem = new UserItem();
            userItem.Email = "validUserEmail";
            userItem.FirstName = "validFirstName";
            userItem.LastName = "validLastName";
            userItem.Role = Role.User;
            userItem.PasswordHash = passwordHash;
            userItem.PasswordSalt = passwordSalt;

            UserItem adminItem = new UserItem();
            adminItem.Email = "validAdminEmail";
            adminItem.FirstName = "validAdminFirstName";
            adminItem.LastName = "validAdminLastName";
            adminItem.Role = Role.Admin;
            adminItem.PasswordHash = passwordHash;
            adminItem.PasswordSalt = passwordSalt;
            mockRepository.Setup(x => x.GetByEmail("validAdminEmail")).ReturnsAsync(adminItem);
            mockRepository.Setup(y => y.GetAllAsync()).ReturnsAsync(new List<UserItem> { adminItem, userItem });

            var user = new ClaimsPrincipal(
                new ClaimsIdentity(new Claim[]{
                    new Claim(ClaimTypes.Name, "validAdminEmail")
                }));

            var controller = new UsersController(mockRepository.Object);
            controller.ControllerContext = new ControllerContext();     
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

            // Act
            IActionResult actionResult = await controller.GetAll();
            Assert.IsType<OkObjectResult>(actionResult);
            OkObjectResult okActionResult = actionResult as OkObjectResult;
            List<UserLoginResponse> ulrList = okActionResult.Value as List<UserLoginResponse>; 
            var ulrArr = ulrList.ToArray();
            Assert.Equal(ulrArr[0].FirstName, adminItem.FirstName);
            Assert.Equal(ulrArr[0].LastName, adminItem.LastName);
            Assert.Equal(ulrArr[0].Email, adminItem.Email);
            Assert.Equal(ulrArr[0].Role, adminItem.Role);
            Assert.Equal(ulrArr[1].FirstName, userItem.FirstName);
            Assert.Equal(ulrArr[1].LastName, userItem.LastName);
            Assert.Equal(ulrArr[1].Email, userItem.Email);
            Assert.Equal(ulrArr[1].Role, userItem.Role);
        }

        [Fact]
        public async void GetByUsername()
        {
            // Arrange
            var mockRepository = new Mock<IUserRepository>();
            Environment.SetEnvironmentVariable("JWT_SECRET", "Some JWT secret for token generation (at least 16chars)");
            byte[] passwordHash = null;
            byte[] passwordSalt = null;
            string password = "validPass";
            CreatePasswordHash(password, out passwordHash, out passwordSalt);
            UserItem userItem = new UserItem();
            userItem.Email = "validUserEmail";
            userItem.FirstName = "validFirstName";
            userItem.LastName = "validLastName";
            userItem.Role = Role.User;
            userItem.PasswordHash = passwordHash;
            userItem.PasswordSalt = passwordSalt;
            mockRepository.Setup(x => x.GetByEmail("validUserEmail")).ReturnsAsync(userItem);

            var user = new ClaimsPrincipal(
                new ClaimsIdentity(new Claim[]{
                    new Claim(ClaimTypes.Name, "validUserEmail")
                }));

            var controller = new UsersController(mockRepository.Object);
            controller.ControllerContext = new ControllerContext();     
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

            // Act
            IActionResult actionResult = await controller.GetByEmail(userItem.Email);
            Assert.IsType<OkObjectResult>(actionResult);
            OkObjectResult okActionResult = actionResult as OkObjectResult;
            UserLoginResponse ulr = okActionResult.Value as UserLoginResponse; 
            Assert.Equal(ulr.FirstName, userItem.FirstName);
            Assert.Equal(ulr.LastName, userItem.LastName);
            Assert.Equal(ulr.Email, userItem.Email);
            Assert.Equal(ulr.Role, userItem.Role);
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