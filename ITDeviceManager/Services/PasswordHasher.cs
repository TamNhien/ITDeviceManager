using System.Security.Cryptography;
using System.Text;
using Isopoh.Cryptography.Argon2;

namespace ITDeviceManager.Services;

/// <summary>
/// Password hashing service.
/// New passwords use Argon2id (Argon2 hybrid addressing) with an encoded PHC string.
/// PBKDF2 verification is retained only so V1.0.0 accounts can be transparently upgraded.
/// </summary>
public static class PasswordHasher
{
    // Argon2id parameters for this desktop application.
    // MemoryCost is expressed in KiB: 65,536 KiB = 64 MiB.
    private const int Argon2MemoryCost = 65_536;
    private const int Argon2TimeCost = 3;
    private const int Argon2Lanes = 2;
    private const int Argon2HashLength = 32;
    private const int Argon2SaltLength = 32;

    // Legacy V1.0.0 PBKDF2 settings. Do not use for new password hashes.
    private const int LegacyPbkdf2Iterations = 100_000;
    private const int LegacyPbkdf2KeySize = 32;

    public static (string Hash, string Salt) HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var salt = RandomNumberGenerator.GetBytes(Argon2SaltLength);

        try
        {
            var config = new Argon2Config
            {
                Type = Argon2Type.HybridAddressing, // Argon2id
                Version = Argon2Version.Nineteen,
                TimeCost = Argon2TimeCost,
                MemoryCost = Argon2MemoryCost,
                Lanes = Argon2Lanes,
                Threads = Math.Min(Argon2Lanes, Math.Max(1, Environment.ProcessorCount)),
                Password = passwordBytes,
                Salt = salt,
                HashLength = Argon2HashLength,
                ClearPassword = true
            };

            var argon2 = new Argon2(config);
            using var hash = argon2.Hash();

            // Salt + parameters are embedded in this standard encoded hash.
            // PasswordSalt remains in the database only for V1.0.0 compatibility.
            return (config.EncodeString(hash.Buffer), string.Empty);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(passwordBytes);
        }
    }

    public static bool Verify(string password, string expectedHash, string? legacySaltBase64)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(expectedHash))
            return false;

        if (IsArgon2idHash(expectedHash))
            return VerifyArgon2id(password, expectedHash);

        return VerifyLegacyPbkdf2(password, expectedHash, legacySaltBase64);
    }

    public static bool NeedsRehash(string hash)
    {
        if (!IsArgon2idHash(hash))
            return true;

        // EncodeString writes the cost parameters into the PHC string.
        // If the policy is strengthened in a future version, successful login
        // will transparently replace older hashes.
        return !hash.Contains($"m={Argon2MemoryCost},t={Argon2TimeCost},p={Argon2Lanes}", StringComparison.Ordinal);
    }

    private static bool IsArgon2idHash(string hash) =>
        hash.StartsWith("$argon2id$", StringComparison.Ordinal);

    private static bool VerifyArgon2id(string password, string encodedHash)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);

        try
        {
            // The library decodes salt/cost/hash from the PHC string and uses
            // a fixed-time comparison for verification.
            return Argon2.Verify(
                encodedHash,
                passwordBytes,
                Math.Min(Argon2Lanes, Math.Max(1, Environment.ProcessorCount)));
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (FormatException)
        {
            return false;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(passwordBytes);
        }
    }

    private static bool VerifyLegacyPbkdf2(string password, string expectedHash, string? saltBase64)
    {
        if (string.IsNullOrWhiteSpace(saltBase64))
            return false;

        try
        {
            var salt = Convert.FromBase64String(saltBase64);
            var expected = Convert.FromBase64String(expectedHash);
            var actual = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                LegacyPbkdf2Iterations,
                HashAlgorithmName.SHA256,
                LegacyPbkdf2KeySize);

            try
            {
                return expected.Length == actual.Length &&
                       CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(actual);
                CryptographicOperations.ZeroMemory(expected);
                CryptographicOperations.ZeroMemory(salt);
            }
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
