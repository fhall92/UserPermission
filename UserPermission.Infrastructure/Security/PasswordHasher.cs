using System.Security.Cryptography;
using System.Text;
using UserPermission.Core.Interfaces;

namespace UserPermission.Infrastructure.Security
{
    public class PasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public bool Verify(string hash, string password)
        {
            var passwordHash = Hash(password);
            return hash == passwordHash;
        }
    }
}