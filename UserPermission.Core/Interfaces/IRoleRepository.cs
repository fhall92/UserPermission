using UserPermission.Core.Entities;

namespace UserPermission.Core.Interfaces
{
    public interface IRoleRepository
    {
        Task<Role?> GetByNameAsync(string name, CancellationToken ct = default);
        Task AddAsync(Role role, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }

}
