using System.Data;
using ITDeviceManager.Data;
using ITDeviceManager.Services;
using Microsoft.EntityFrameworkCore;

var skipDatabase = args.Any(x => string.Equals(x, "--skip-db", StringComparison.OrdinalIgnoreCase));
var failures = new List<string>();

Console.WriteLine("ITDeviceManager self-test");
Console.WriteLine(new string('=', 40));

Run("PasswordPolicy accepts a strong-length password", () =>
{
    var error = PasswordPolicy.Validate("Example@123!2026");
    Assert(error is null, error ?? "Unexpected password-policy rejection.");
});

Run("PasswordPolicy rejects a short password", () =>
{
    Assert(PasswordPolicy.Validate("short") is not null, "Short password was accepted.");
});

Run("Email validator accepts a normal address", () =>
{
    Assert(EmailAddressValidator.IsValid("user@example.com"), "Valid email was rejected.");
});

Run("Email validator rejects malformed input", () =>
{
    Assert(!EmailAddressValidator.IsValid("not-an-email"), "Malformed email was accepted.");
});

Run("Argon2id hashes and verifies passwords", () =>
{
    const string password = "Example@123!2026";
    var (hash, salt) = PasswordHasher.HashPassword(password);

    Assert(hash.StartsWith("$argon2id$", StringComparison.Ordinal), "Hash is not Argon2id PHC format.");
    Assert(string.IsNullOrEmpty(salt), "New Argon2id hashes should not need a separate salt column.");
    Assert(PasswordHasher.Verify(password, hash, salt), "Correct password did not verify.");
    Assert(!PasswordHasher.Verify("WrongPassword@123", hash, salt), "Wrong password unexpectedly verified.");
});

try
{
    EnvFileLoader.Load();
    Console.WriteLine(EnvFileLoader.LoadedFilePath is null
        ? "[INFO] .env was not found; environment/default settings will be used."
        : $"[PASS] .env located: {EnvFileLoader.LoadedFilePath}");
}
catch (Exception ex)
{
    failures.Add($".env loader: {ex.Message}");
    Console.WriteLine($"[FAIL] .env loader: {ex.Message}");
}

if (!skipDatabase)
{
    await RunAsync("Database initialize + V1.2 schema verification", async () =>
    {
        await using var db = new AppDbContext();
        await DbInitializer.InitializeAsync(db);

        var connection = db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
SELECT CASE WHEN
    COL_LENGTH(N'dbo.Users', N'Email') IS NOT NULL
    AND OBJECT_ID(N'dbo.PasswordResetTokens', N'U') IS NOT NULL
    AND EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_Users_Email'
          AND object_id = OBJECT_ID(N'dbo.Users'))
THEN 1 ELSE 0 END;
""";

        var result = Convert.ToInt32(await command.ExecuteScalarAsync());
        Assert(result == 1, "Database schema is missing Users.Email, PasswordResetTokens, or IX_Users_Email.");
    });
}
else
{
    Console.WriteLine("[SKIP] Database test (--skip-db).");
}

Console.WriteLine();
if (failures.Count == 0)
{
    Console.WriteLine("SELF-TEST PASSED");
    return 0;
}

Console.Error.WriteLine($"SELF-TEST FAILED: {failures.Count} check(s) failed.");
foreach (var failure in failures)
    Console.Error.WriteLine($"  - {failure}");
return 1;

void Run(string name, Action action)
{
    try
    {
        action();
        Console.WriteLine($"[PASS] {name}");
    }
    catch (Exception ex)
    {
        failures.Add($"{name}: {ex.Message}");
        Console.WriteLine($"[FAIL] {name}: {ex.Message}");
    }
}

async Task RunAsync(string name, Func<Task> action)
{
    try
    {
        await action();
        Console.WriteLine($"[PASS] {name}");
    }
    catch (Exception ex)
    {
        failures.Add($"{name}: {ex.GetBaseException().Message}");
        Console.WriteLine($"[FAIL] {name}: {ex.GetBaseException().Message}");
    }
}

static void Assert(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}
