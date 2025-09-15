using UserPermission.Core.DTOs;

namespace UserPermission.Core.Interfaces
{
    public interface IUserService
    {
        Task<UserResponseDto> RegisterAsync(UserCreateDto dto, CancellationToken ct = default);
        Task<UserResponseDto?> AuthenticateAsync(LoginDto dto, CancellationToken ct = default);
        Task AssignRoleAsync(Guid userId, string roleName, CancellationToken ct = default);
        Task<UserResponseDto?> GetByIdAsync(Guid userId, CancellationToken ct = default);
    }
}
