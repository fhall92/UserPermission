using Microsoft.AspNetCore.Mvc;
using UserPermission.Api.Controllers;
using UserPermission.Core.DTOs;
using UserPermission.Core.Entities;
using UserPermission.Core.Interfaces;
using Moq;

namespace UserPermission.UnitTests.Controllers
{
    public class AuthControllerTests
    {
        [Fact]
        public async Task Login_ReturnsOk_WhenValid()
        {
            var userService = new Mock<IUserService>();
            var user = new User { Id = Guid.NewGuid(), Email = "test@testmail.com", Name = "John Smith" };

            var responseDto = new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Roles = new List<string>()
            };

            userService.Setup(s => s.AuthenticateAsync(
                It.Is<LoginDto>(dto => dto.Email == user.Email),
                default))
                .ReturnsAsync(responseDto);

            var controller = new AuthController(userService.Object);
            var res = await controller.Login(new LoginDto { Email = "test@testmail.com", Password = "password" }) as OkObjectResult;

            Assert.NotNull(res);
            Assert.Equal(200, res!.StatusCode);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenInvalid()
        {
            var userService = new Mock<IUserService>();
            userService.Setup(s => s.AuthenticateAsync(It.IsAny<LoginDto>(), default)).ReturnsAsync((UserResponseDto?)null);

            var controller = new AuthController(userService.Object);
            var res = await controller.Login(new LoginDto { Email = "x@x.com", Password = "no" });
            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public async Task Login_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            var userService = new Mock<IUserService>();
            var controller = new AuthController(userService.Object);
            controller.ModelState.AddModelError("Email", "Required");

            var res = await controller.Login(new LoginDto { Email = "", Password = "password" });
            Assert.IsType<BadRequestObjectResult>(res);
        }

        [Fact]
        public async Task Login_ReturnsBadRequest_WhenLoginDtoIsNull()
        {
            var userService = new Mock<IUserService>();
            var controller = new AuthController(userService.Object);

            var res = await controller.Login(null!);
            Assert.IsType<BadRequestObjectResult>(res);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenPasswordIsMissing()
        {
            var userService = new Mock<IUserService>();
            var controller = new AuthController(userService.Object);

            var res = await controller.Login(new LoginDto { Email = "test@testmail.com", Password = "" });
            Assert.IsType<UnauthorizedResult>(res);
        }
    }
}
