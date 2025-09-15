using UserPermission.Core.DTOs;
using UserPermission.Core.Entities;
using UserPermission.Core.Interfaces;

namespace UserPermission.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPasswordHasher _passwordHasher;

        public UserService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<UserResponseDto> RegisterAsync(UserCreateDto userCreateDto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(userCreateDto.Name)) 
                throw new ArgumentException("Name is required");

            if (string.IsNullOrWhiteSpace(userCreateDto.Email)) 
                throw new ArgumentException("Email is required");

            if (string.IsNullOrWhiteSpace(userCreateDto.Password) || userCreateDto.Password.Length < 6)
                throw new ArgumentException("Password must be at least 6 characters");

            var existing = await _userRepository.GetByEmailAsync(userCreateDto.Email);
            if (existing != null)
                throw new InvalidOperationException("User already exists with this email");

            var hashedPassword = _passwordHasher.Hash(userCreateDto.Password);

            var user = new User()
            {
                Id = Guid.NewGuid(),
                Name = userCreateDto.Name,
                Email = userCreateDto.Email,
                PasswordHash = hashedPassword
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return new UserResponseDto()
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
            };
        }

        public async Task<UserResponseDto?> AuthenticateAsync(LoginDto loginDto, CancellationToken ct = default)
        {
            var user = await _userRepository.GetByEmailAsync(loginDto.Email, ct);
            if (user == null) return null;

            var isValid = _passwordHasher.Verify(user.PasswordHash, loginDto.Password);
            return isValid ? 
                new UserResponseDto()
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
                } : null;
        }

        public async Task AssignRoleAsync(Guid userId, string roleName, CancellationToken ct = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, ct);
            if (user == null) throw new KeyNotFoundException("User not found");

            var role = await _roleRepository.GetByNameAsync(roleName, ct);
            if (role == null)
            {
                role = new Role() { Name = roleName};
                await _roleRepository.AddAsync(role, ct);
                await _roleRepository.SaveChangesAsync(ct);
            }

            user.UserRoles.Add(new UserRole()
            {
                UserId = user.Id,
                RoleId = role.Id,
                Role = role,
                User = user
            });

            await _userRepository.SaveChangesAsync(ct);
        }

        public async Task<UserResponseDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var user = await _userRepository.GetByIdAsync(id, ct);

                return user != null ? new UserResponseDto()
                { 
                    Email = user.Email,
                    Id = user.Id,
                    Name = user.Name,
                    Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
                }: null;
        }
    }
}
