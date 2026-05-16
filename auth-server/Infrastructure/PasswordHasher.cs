using System.Security.Cryptography;
using System.Text;

namespace AuthServer.Infrastructure;

/// <summary>
/// SSD: replaces one-shot SHA256 (fast to crack) with PBKDF2 — slows offline guessing while keeping verification simple for this sample.
/// Legacy SHA256-only hashes (no <c>v1$</c> prefix) remain verifiable so existing dev databases keep working until users re-hash on next password change.
/// </summary>
public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;
    private const string Prefix = "v1$";

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var subKey = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return $"{Prefix}{Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(subKey)}";
    }

    public static bool Verify(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(storedHash))
            return false;

        if (storedHash.StartsWith(Prefix, StringComparison.Ordinal))
            return VerifyPbkdf2(password, storedHash);

        return VerifyLegacySha256(password, storedHash);
    }

    private static bool VerifyLegacySha256(string password, string storedHash)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        var candidate = Convert.ToBase64String(bytes);
        var a = Encoding.UTF8.GetBytes(candidate);
        var b = Encoding.UTF8.GetBytes(storedHash);
        if (a.Length != b.Length)
            return false;

        return CryptographicOperations.FixedTimeEquals(a, b);
    }

    private static bool VerifyPbkdf2(string password, string storedHash)
    {
        // v1$iterations$base64salt$base64subkey
        var parts = storedHash.Split('$', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4 || parts[0] != "v1")
            return false;

        if (!int.TryParse(parts[1], out var iterations) || iterations < 10_000)
            return false;

        var salt = Convert.FromBase64String(parts[2]);
        var expectedSubKey = Convert.FromBase64String(parts[3]);

        var actualSubKey = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expectedSubKey.Length);

        return CryptographicOperations.FixedTimeEquals(actualSubKey, expectedSubKey);
    }
}
