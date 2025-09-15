using UserPermission.Infrastructure.Security;
using Xunit;

namespace UserPermission.UnitTests.Security
{
    public class PasswordHasherTests
    {
        private readonly PasswordHasher _hasher = new();

        [Fact]
        public void Hash_SamePassword_ProducesSameHash()
        {
            var hash1 = _hasher.Hash("password123");
            var hash2 = _hasher.Hash("password123");
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void Hash_DifferentPasswords_ProduceDifferentHashes()
        {
            var hash1 = _hasher.Hash("password123");
            var hash2 = _hasher.Hash("differentPassword");
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void Verify_CorrectPassword_ReturnsTrue()
        {
            var password = "mySecret";
            var hash = _hasher.Hash(password);
            var result = _hasher.Verify(hash, password);
            Assert.True(result);
        }

        [Fact]
        public void Verify_WrongPassword_ReturnsFalse()
        {
            var hash = _hasher.Hash("rightPassword");
            var result = _hasher.Verify(hash, "wrongPassword");
            Assert.False(result);
        }
    }
}