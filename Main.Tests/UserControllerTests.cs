using System;
using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Main.Persistence;
using Main.Controllers;

namespace Main.Tests
{
    public class UsersControllerTests
    {
        [Fact]
        public /*async*/ void GetReturnsProductWithSameId()
        {
            // // Arrange
            // var mockRepository = new Mock<IUserRepository>();
            // mockRepository.Setup(x => x.GetByUsername("someUser"))
            //     .ReturnsAsync(new UserItem { Username = "someuser" });

            // var controller = new UsersController(mockRepository.Object);

            // // Act
            // IActionResult actionResult = await controller.GetByUsername("someUser");
            // Assert.IsType<OkObjectResult>(actionResult);

            // // Assert
            // Assert.Equal(1,1);
        }
    }
}