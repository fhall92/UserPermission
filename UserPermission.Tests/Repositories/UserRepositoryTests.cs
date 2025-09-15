using Microsoft.EntityFrameworkCore;
using UserPermission.Infrastructure.Data;
using UserPermission.Infrastructure.Repositories;
using UserPermission.Core.Entities;

namespace UserPermission.UnitTests.Repositories
{
    public class UserRepositoryTests
    {
        private AppDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task AddAndGetByEmailAndId_Works()
        {
            var ctx = CreateContext(nameof(AddAndGetByEmailAndId_Works));
            var repo = new UserRepository(ctx);

            var user = new User { Id = Guid.NewGuid(), Name = "X", Email = "x@x.com", PasswordHash = "h" };
            await repo.AddAsync(user);
            await repo.SaveChangesAsync();

            var byEmail = await repo.GetByEmailAsync("x@x.com");
            Assert.NotNull(byEmail);
            Assert.Equal(user.Id, byEmail!.Id);

            var byId = await repo.GetByIdAsync(user.Id);
            Assert.NotNull(byId);
            Assert.Equal("X", byId!.Name);
        }

        [Fact]
        public async Task IncludesRoles_WhenPresent()
        {
            var ctx = CreateContext(nameof(IncludesRoles_WhenPresent));
            var repo = new UserRepository(ctx);

            var role = new Role { Id = Guid.NewGuid(), Name = "r" };
            var user = new User { Id = Guid.NewGuid(), Name = "U", Email = "u@u.com", PasswordHash = "h" };
            var ur = new UserRole { UserId = user.Id, RoleId = role.Id, Role = role, User = user };

            await ctx.Roles.AddAsync(role);
            await ctx.Users.AddAsync(user);
            await ctx.UserRoles.AddAsync(ur);
            await ctx.SaveChangesAsync();

            var fetched = await repo.GetByIdAsync(user.Id);
            Assert.NotNull(fetched);
            Assert.NotEmpty(fetched!.UserRoles);
            Assert.Equal("r", fetched.UserRoles.First().Role.Name);
        }

        [Fact]
        public async Task SaveChangesAsync_PersistsChanges()
        {
            var ctx = CreateContext(nameof(SaveChangesAsync_PersistsChanges));
            var repo = new UserRepository(ctx);

            var user = new User { Id = Guid.NewGuid(), Name = "Y", Email = "y@y.com", PasswordHash = "h" };
            await repo.AddAsync(user);
            await repo.SaveChangesAsync();

            // Use a new context to verify persistence
            var ctx2 = CreateContext(nameof(SaveChangesAsync_PersistsChanges));
            var repo2 = new UserRepository(ctx2);
            var byId = await repo2.GetByIdAsync(user.Id);
            Assert.NotNull(byId);
            Assert.Equal("Y", byId!.Name);
        }

        [Fact]
        public async Task GetByEmailAsync_ReturnsNull_WhenUserMissing()
        {
            var ctx = CreateContext(nameof(GetByEmailAsync_ReturnsNull_WhenUserMissing));
            var repo = new UserRepository(ctx);

            var result = await repo.GetByEmailAsync("notfound@x.com");
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenUserMissing()
        {
            var ctx = CreateContext(nameof(GetByIdAsync_ReturnsNull_WhenUserMissing));
            var repo = new UserRepository(ctx);

            var result = await repo.GetByIdAsync(Guid.NewGuid());
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_DoesNotPersistUntilSaveChanges()
        {
            var ctx = CreateContext(nameof(AddAsync_DoesNotPersistUntilSaveChanges));
            var repo = new UserRepository(ctx);

            var user = new User { Id = Guid.NewGuid(), Name = "Z", Email = "z@z.com", PasswordHash = "h" };
            await repo.AddAsync(user);

            // Not saved yet
            var byEmail = await repo.GetByEmailAsync("z@z.com");
            Assert.Null(byEmail);

            await repo.SaveChangesAsync();

            byEmail = await repo.GetByEmailAsync("z@z.com");
            Assert.NotNull(byEmail);
        }

        [Fact]
        public async Task GetByEmailAsync_IsCaseInsensitive()
        {
            var ctx = CreateContext(nameof(GetByEmailAsync_IsCaseInsensitive));
            var repo = new UserRepository(ctx);

            var user = new User { Id = Guid.NewGuid(), Name = "Case", Email = "Case@Test.com", PasswordHash = "h" };
            await repo.AddAsync(user);
            await repo.SaveChangesAsync();

            var byEmail = await repo.GetByEmailAsync("case@test.com");
            Assert.NotNull(byEmail);
            Assert.Equal(user.Id, byEmail!.Id);
        }
    }
}