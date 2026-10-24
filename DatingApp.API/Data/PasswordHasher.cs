using System.Security.Cryptography;
using System.Text;

namespace DatingApp.API.Data;

/// <summary>HMAC-SHA512 password hashing compatible with the existing User table.</summary>
public static class PasswordHasher
{
    public const int HashBytes = 64;   // SHA-512 output
    public const int SaltBytes = 128;  // default HMACSHA512 key size

    public static (byte[] Hash, byte[] Salt) Hash(string password)
    {
        using var hmac = new HMACSHA512();
        return (hmac.ComputeHash(Encoding.UTF8.GetBytes(password)), hmac.Key);
    }

    /// <summary>
    /// The PasswordHash/PasswordSalt columns are binary(512), so SQL Server pads
    /// stored values with zeros. Only the first 64/128 bytes are real data;
    /// using the padded salt as the HMAC key would give a different hash.
    /// </summary>
    public static bool Verify(string password, byte[]? storedHash, byte[]? storedSalt)
    {
        if (storedHash == null || storedSalt == null || storedHash.Length < HashBytes || storedSalt.Length < SaltBytes)
            return false;

        using var hmac = new HMACSHA512(storedSalt.AsSpan(0, SaltBytes).ToArray());
        var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return CryptographicOperations.FixedTimeEquals(computed, storedHash.AsSpan(0, HashBytes));
    }
}
