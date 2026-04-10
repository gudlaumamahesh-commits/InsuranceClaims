using System.Security.Cryptography;

namespace InsuranceClaims.Helpers
{
    // Provides HMACSHA512-based password hashing and verification.
    public static class PasswordHelper
    {
        //Creates a hashed password with a random salt.
        public static void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            using var hmac = new HMACSHA512();
            salt = hmac.Key;                          
            hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }
        // Verifies a plain password against a stored hash and salt.
        public static bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            using var hmac = new HMACSHA512(storedSalt);
            var computed = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return computed.SequenceEqual(storedHash);
        }
    }
}
