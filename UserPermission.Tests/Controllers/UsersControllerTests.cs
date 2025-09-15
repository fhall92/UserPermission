using Microsoft.AspNetCore.Mvc;
using Moq;
using UserPermission.Api.Controllers;
using UserPermission.Core.DTOs;
using UserPermission.Core.Entities;
using UserPermission.Core.Interfaces;

namespace UserPermission.UnitTests.Controllers
{
    public class UsersControllerTests
    {
        [Fact]
        public async Task Create_ReturnsCreated()
        {
            var userService = new Mock<IUserService>();
            var user = new User { Id = Guid.NewGuid(), Name = "A", Email = "a@b.com" };

            var userCreateDto = new UserCreateDto
            {
                Name = user.Name,
                Email = user.Email,
                Password = "password",
            };

            var response = new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Roles = new List<string>()
            };

            userService.Setup(s => s.RegisterAsync(userCreateDto, default))
                .ReturnsAsync(response);

            var controller = new UsersController(userService.Object);

            var result = await controller.Create(userCreateDto) as CreatedAtActionResult;

            Assert.NotNull(result);
            Assert.Equal(nameof(UsersController.GetById), result!.ActionName);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            var userService = new Mock<IUserService>();
            var controller = new UsersController(userService.Object);
            controller.ModelState.AddModelError("Email", "Required");

            var result = await controller.Create(new UserCreateDto { Name = "A", Email = "", Password = "password" });
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Create_ReturnsConflict_WhenUserAlreadyExists()
        {
            var userService = new Mock<IUserService>();
            userService.Setup(s => s.RegisterAsync(It.IsAny<UserCreateDto>(), default))
                .ThrowsAsync(new InvalidOperationException("User already exists"));

            var controller = new UsersController(userService.Object);

            var result = await controller.Create(new UserCreateDto { Name = "A", Email = "a@b.com", Password = "password" });
            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenMissing()
        {
            var userService = new Mock<IUserService>();
            userService.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((UserResponseDto?)null);
            var controller = new UsersController(userService.Object);

            var res = await controller.GetById(Guid.NewGuid());
            Assert.IsType<NotFoundResult>(res);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenUserExists()
        {
            var userService = new Mock<IUserService>();
            var userId = Guid.NewGuid();
            var response = new UserResponseDto
            {
                Id = userId,
                Name = "A",
                Email = "a@b.com",
                Roles = new List<string>()
            };
            userService.Setup(s => s.GetByIdAsync(userId, default)).ReturnsAsync(response);

            var controller = new UsersController(userService.Object);

            var result = await controller.GetById(userId);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task AssignRole_ReturnsNoContent()
        {
            var userService = new Mock<IUserService>();
            userService.Setup(s => s.AssignRoleAsync(It.IsAny<Guid>(), "admin", default)).Returns(Task.CompletedTask);

            var controller = new UsersController(userService.Object);
            var res = await controller.AssignRole(Guid.NewGuid(), new AssignRoleDto { RoleName = "admin" });
            Assert.IsType<NoContentResult>(res);
        }

        [Fact]
        public async Task AssignRole_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            var userService = new Mock<IUserService>();
            var controller = new UsersController(userService.Object);
            controller.ModelState.AddModelError("RoleName", "Required");

            var result = await controller.AssignRole(Guid.NewGuid(), new AssignRoleDto { RoleName = "" });
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task AssignRole_ReturnsNotFound_WhenUserDoesNotExist()
        {
            var userService = new Mock<IUserService>();
            userService.Setup(s => s.AssignRoleAsync(It.IsAny<Guid>(), It.IsAny<string>(), default))
                .ThrowsAsync(new KeyNotFoundException());

            var controller = new UsersController(userService.Object);

            var result = await controller.AssignRole(Guid.NewGuid(), new AssignRoleDto { RoleName = "admin" });
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenArgumentExceptionThrown()
        {
            var userService = new Mock<IUserService>();
            userService.Setup(s => s.RegisterAsync(It.IsAny<UserCreateDto>(), default))
                .ThrowsAsync(new ArgumentException("Invalid input"));

            var controller = new UsersController(userService.Object);

            var result = await controller.Create(new UserCreateDto { Name = "A", Email = "bad", Password = "pw" });
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Invalid input", badRequest.Value?.ToString());
        }

        [Fact]
        public async Task AssignRole_ReturnsBadRequest_WhenArgumentExceptionThrown()
        {
            var userService = new Mock<IUserService>();
            userService.Setup(s => s.AssignRoleAsync(It.IsAny<Guid>(), It.IsAny<string>(), default))
                .ThrowsAsync(new ArgumentException("Invalid role"));

            var controller = new UsersController(userService.Object);

            var result = await controller.AssignRole(Guid.NewGuid(), new AssignRoleDto { RoleName = "badrole" });
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Invalid role", badRequest.Value?.ToString());
        }
    }
}
