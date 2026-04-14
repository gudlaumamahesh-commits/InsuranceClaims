using System.Security.Cryptography;

namespace InsuranceClaims.Helpers
{
    public static class PasswordHelper
    {
        public static void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            using var hmac = new HMACSHA512();
            salt = hmac.Key;                          
            hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }
        public static bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            using var hmac = new HMACSHA512(storedSalt);
            var computed = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return computed.SequenceEqual(storedHash);
        }
    }
}
