using System;
using System.Security.Cryptography;
using System.Text;

namespace Pos_System.Services
{
    internal static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100000;
        private const string Prefix = "PBKDF2";

        public static string Hash(string password)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            byte[] salt = new byte[SaltSize];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create()) rng.GetBytes(salt);
            byte[] hash = DeriveHash(password, salt, Iterations);
            return string.Join("$", Prefix, "1", Iterations.ToString(), Convert.ToBase64String(salt), Convert.ToBase64String(hash));
        }

        public static bool Verify(string password, string storedValue)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedValue)) return false;
            if (IsPbkdf2(storedValue)) return VerifyPbkdf2(password, storedValue);

            bool legacyEnabled = POS_System.Program.SettingsManager.GetBoolSetting("legacy_password_fallback_enabled", true);
            if (!legacyEnabled) return false;

            return ConstantTimeEquals(storedValue, password) || ConstantTimeEquals(storedValue, Sha256Hex(password));
        }

        public static bool NeedsRehash(string storedValue) { return string.IsNullOrEmpty(storedValue) || !IsPbkdf2(storedValue); }
        public static bool IsPbkdf2(string storedValue) { return !string.IsNullOrEmpty(storedValue) && storedValue.StartsWith(Prefix + "$", StringComparison.Ordinal); }

        private static bool VerifyPbkdf2(string password, string storedValue)
        {
            string[] parts = storedValue.Split('$');
            if (parts.Length != 5 || parts[0] != Prefix) return false;
            if (!int.TryParse(parts[2], out int iterations) || iterations < 10000) return false;
            try
            {
                byte[] salt = Convert.FromBase64String(parts[3]);
                byte[] expectedHash = Convert.FromBase64String(parts[4]);
                byte[] actualHash = DeriveHash(password, salt, iterations);
                return ConstantTimeEquals(actualHash, expectedHash);
            }
            catch (FormatException) { return false; }
        }

        private static byte[] DeriveHash(string password, byte[] salt, int iterations)
        {
            using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations)) return pbkdf2.GetBytes(HashSize);
        }

        private static string Sha256Hex(string value)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
                StringBuilder builder = new StringBuilder(bytes.Length * 2);
                foreach (byte b in bytes) builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        private static bool ConstantTimeEquals(string left, string right) { return ConstantTimeEquals(Encoding.UTF8.GetBytes(left), Encoding.UTF8.GetBytes(right)); }
        private static bool ConstantTimeEquals(byte[] left, byte[] right)
        {
            int difference = left.Length ^ right.Length;
            int length = Math.Min(left.Length, right.Length);
            for (int i = 0; i < length; i++) difference |= left[i] ^ right[i];
            return difference == 0;
        }
    }
}
