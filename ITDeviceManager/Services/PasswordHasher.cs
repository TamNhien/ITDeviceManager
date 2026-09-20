using System.Security.Cryptography;
using System.Text;
using Isopoh.Cryptography.Argon2;

namespace ITDeviceManager.Services;

/// <summary>
/// Password hashing service. V1.3.0 stores only the Argon2id PHC string.
/// The PHC string already contains the algorithm version, cost parameters,
/// random salt and derived hash, so a separate PasswordSalt column is unnecessary.
/// </summary>
public static class PasswordHasher
{
    private const int Argon2MemoryCost = 65_536; // KiB = 64 MiB
    private const int Argon2TimeCost = 3;
    private const int Argon2Lanes = 2;
    private const int Argon2HashLength = 32;
    private const int Argon2SaltLength = 32;

    public static string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var salt = RandomNumberGenerator.GetBytes(Argon2SaltLength);

        try
        {
            var config = new Argon2Config
            {
                Type = Argon2Type.HybridAddressing,
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
            return config.EncodeString(hash.Buffer);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(passwordBytes);
            CryptographicOperations.ZeroMemory(salt);
        }
    }

    public static bool Verify(string password, string expectedHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(expectedHash) || !IsArgon2idHash(expectedHash))
            return false;

        var passwordBytes = Encoding.UTF8.GetBytes(password);

        try
        {
            return Argon2.Verify(
                expectedHash,
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

    public static bool NeedsRehash(string hash)
    {
        if (!IsArgon2idHash(hash))
            return true;

        return !hash.Contains(
            $"m={Argon2MemoryCost},t={Argon2TimeCost},p={Argon2Lanes}",
            StringComparison.Ordinal);
    }

    public static bool IsArgon2idHash(string hash) =>
        !string.IsNullOrWhiteSpace(hash) && hash.StartsWith("$argon2id$", StringComparison.Ordinal);
}
