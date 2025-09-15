using Microsoft.EntityFrameworkCore;
using UserPermission.Infrastructure.Data;
using UserPermission.Infrastructure.Repositories;
using UserPermission.Core.Entities;

namespace UserPermission.UnitTests.Repositories
{
    public class RoleRepositoryTests
    {
        private AppDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task AddAndGetByName_Works()
        {
            var ctx = CreateContext(nameof(AddAndGetByName_Works));
            var repo = new RoleRepository(ctx);

            var role = new Role { Id = Guid.NewGuid(), Name = "Admin" };
            await repo.AddAsync(role);
            await repo.SaveChangesAsync();

            var fetched = await repo.GetByNameAsync("Admin");
            Assert.NotNull(fetched);
            Assert.Equal(role.Id, fetched!.Id);
        }

        [Fact]
        public async Task GetByNameAsync_IsCaseInsensitive()
        {
            var ctx = CreateContext(nameof(GetByNameAsync_IsCaseInsensitive));
            var repo = new RoleRepository(ctx);

            var role = new Role { Id = Guid.NewGuid(), Name = "Manager" };
            await repo.AddAsync(role);
            await repo.SaveChangesAsync();

            var fetched = await repo.GetByNameAsync("manager");
            Assert.NotNull(fetched);
            Assert.Equal(role.Id, fetched!.Id);
        }

        [Fact]
        public async Task GetByNameAsync_ReturnsNull_WhenRoleMissing()
        {
            var ctx = CreateContext(nameof(GetByNameAsync_ReturnsNull_WhenRoleMissing));
            var repo = new RoleRepository(ctx);

            var fetched = await repo.GetByNameAsync("NotExist");
            Assert.Null(fetched);
        }

        [Fact]
        public async Task AddAsync_DoesNotPersistUntilSaveChanges()
        {
            var ctx = CreateContext(nameof(AddAsync_DoesNotPersistUntilSaveChanges));
            var repo = new RoleRepository(ctx);

            var role = new Role { Id = Guid.NewGuid(), Name = "Temp" };
            await repo.AddAsync(role);

            var fetched = await repo.GetByNameAsync("Temp");
            Assert.Null(fetched);

            await repo.SaveChangesAsync();

            fetched = await repo.GetByNameAsync("Temp");
            Assert.NotNull(fetched);
        }
    }
}
