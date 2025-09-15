using Microsoft.EntityFrameworkCore;
using UserPermission.Core.Entities;
using UserPermission.Infrastructure.Data;

namespace UserPermission.Infrastructure.Repositories
{
    public class RoleRepository : Core.Interfaces.IRoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Role?> GetByNameAsync(string name, CancellationToken ct = default)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Name.ToLower() == name.ToLower(), ct);
        }

        public async Task AddAsync(Role role, CancellationToken ct = default)
        {
            await _context.Roles.AddAsync(role, ct);
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _context.SaveChangesAsync(ct);
        }
    }
}
