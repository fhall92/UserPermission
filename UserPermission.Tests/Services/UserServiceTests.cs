using Moq;
using UserPermission.Core.Entities;
using UserPermission.Core.Services;
using UserPermission.Core.Interfaces;
using UserPermission.Core.DTOs;

namespace UserPermission.UnitTests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepo = new();
        private readonly Mock<IRoleRepository> _roleRepo = new();
        private readonly Mock<IPasswordHasher> _hasher = new();
        private readonly UserService _service;

        public UserServiceTests()
        {
            _service = new UserService(_userRepo.Object, _roleRepo.Object, _hasher.Object);
        }

        [Fact]
        public async Task RegisterAsync_CreatesUser_WhenEmailNotExists()
        {
            // Arrange
            _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);
            _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed");

            User captured = null!;
            _userRepo.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Callback<User, CancellationToken>((u, ct) => captured = u)
                .Returns(Task.CompletedTask);

            var userCreateDto = new UserCreateDto
            {
                Name = "Joe",
                Email = "joe@testmail.com",
                Password = "password",
            };

            // Act
            var result = await _service.RegisterAsync(userCreateDto);

            // Assert
            _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
            _userRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.NotNull(captured);
            Assert.Equal("Joe", captured.Name);
            Assert.Equal("joe@testmail.com", captured.Email);
            Assert.Equal("hashed", captured.PasswordHash);
        }

        [Fact]
        public async Task RegisterAsync_Throws_WhenEmailExists()
        {
            _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User { Email = "exists@example.com" });

            var userCreateDto = new UserCreateDto()
            {
                Name = "Existing User",
                Email = "exists@example.com",
                Password = "password"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.RegisterAsync(userCreateDto));
        }

        [Fact]
        public async Task RegisterAsync_Throws_WhenNameMissing()
        {
            var dto = new UserCreateDto { Name = "", Email = "a@b.com", Password = "password" };
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.RegisterAsync(dto));
            Assert.Contains("Name", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task RegisterAsync_Throws_WhenEmailInvalid()
        {
            var dto = new UserCreateDto { Name = "A", Email = "", Password = "password" };
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.RegisterAsync(dto));
            Assert.Contains("Email", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task RegisterAsync_Throws_WhenPasswordTooShort()
        {
            var dto = new UserCreateDto { Name = "A", Email = "a@b.com", Password = "123" };
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.RegisterAsync(dto));
            Assert.Contains("Password", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AuthenticateAsync_ReturnsUser_WhenPasswordMatches()
        {
            var user = new User { Id = Guid.NewGuid(), Email = "u@example.com", PasswordHash = "h" };
            _userRepo.Setup(r => r.GetByEmailAsync("u@example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _hasher.Setup(h => h.Verify("h", "pw")).Returns(true);

            var loginDto = new LoginDto
            {
                Password = "pw",
                Email = "u@example.com"
            };

            var result = await _service.AuthenticateAsync(loginDto);
            Assert.NotNull(result);
            Assert.Equal(user.Email, result!.Email);
        }

        [Fact]
        public async Task AuthenticateAsync_ReturnsNull_WhenPasswordMismatch()
        {
            var user = new User { Email = "jane@example.com", PasswordHash = "hashedPassword" };
            _userRepo.Setup(r => r.GetByEmailAsync("u@example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _hasher.Setup(h => h.Verify("password", "hashedPassword")).Returns(false);

            var loginDto = new LoginDto
            {
                Password = "password",
                Email = "jane@example.com"
            };

            var result = await _service.AuthenticateAsync(loginDto);
            Assert.Null(result);
        }

        [Fact]
        public async Task AuthenticateAsync_ReturnsNull_WhenUserNotFound()
        {
            _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var loginDto = new LoginDto { Email = "notfound@x.com", Password = "pw" };
            var result = await _service.AuthenticateAsync(loginDto);
            Assert.Null(result);
        }

        [Fact]
        public async Task AssignRoleAsync_CreatesRoleIfMissing_AndAssignsToUser()
        {
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Email = "a@b.com" };
            _userRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            _roleRepo.Setup(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Role?)null);
            _roleRepo.Setup(r => r.AddAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            // Act
            await _service.AssignRoleAsync(userId, "admin");

            // Assert
            _roleRepo.Verify(r => r.AddAsync(It.Is<Role>(x => x.Name == "admin"), It.IsAny<CancellationToken>()), Times.Once);
            _userRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AssignRoleAsync_Throws_WhenUserMissing()
        {
            _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.AssignRoleAsync(Guid.NewGuid(), "role"));
        }

        [Fact]
        public async Task AssignRoleAsync_DoesNotCreateRole_IfRoleExists()
        {
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Email = "a@b.com", UserRoles = new List<UserRole>() };
            var role = new Role { Id = Guid.NewGuid(), Name = "admin" };

            _userRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _roleRepo.Setup(r => r.GetByNameAsync("admin", It.IsAny<CancellationToken>())).ReturnsAsync(role);

            await _service.AssignRoleAsync(userId, "admin");

            _roleRepo.Verify(r => r.AddAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Never);
            _userRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.Contains(user.UserRoles, ur => ur.RoleId == role.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsUser_WhenFound()
        {
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Name = "Test",
                Email = "test@x.com",
                UserRoles = new List<UserRole>
                {
                    new UserRole { Role = new Role { Name = "admin" } }
                }
            };
            _userRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            var result = await _service.GetByIdAsync(userId);
            Assert.NotNull(result);
            Assert.Equal(userId, result!.Id);
            Assert.Contains("admin", result.Roles);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

            var result = await _service.GetByIdAsync(Guid.NewGuid());
            Assert.Null(result);
        }
    }
}
