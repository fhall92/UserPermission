﻿using UserPermission.Core.Entities;

namespace UserPermission.Infrastructure.Repositories
{
    public interface IUserRepository
    {
        Task AddAsync(User user, CancellationToken ct = default);
        Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }

}
