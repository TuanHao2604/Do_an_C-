using System;
using System.Security.Cryptography;

namespace TravelGuideApp.Services
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100_000;

        public static string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

            return $"pbkdf2${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
        }

        public static bool Verify(string password, string hash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
                return false;

            var parts = hash.Split('$');
            if (parts.Length != 4 || parts[0] != "pbkdf2")
                return false;

            if (!int.TryParse(parts[1], out var iterations))
                return false;

            var salt = Convert.FromBase64String(parts[2]);
            var key = Convert.FromBase64String(parts[3]);
            var keyToCheck = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, key.Length);
            return CryptographicOperations.FixedTimeEquals(keyToCheck, key);
        }
    }
}
