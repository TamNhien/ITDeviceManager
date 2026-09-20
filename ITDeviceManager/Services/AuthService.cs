using ITDeviceManager.Data;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Services;

public class AuthService
{
    public async Task<bool> LoginAsync(string username, string password)
    {
        await using var db = new AppDbContext();
        var normalizedUsername = username.Trim();

        var user = await db.Users
            .Include(x => x.Role)
            .SingleOrDefaultAsync(x => x.Username == normalizedUsername && x.IsActive);

        if (user is null || !PasswordHasher.Verify(password, user.PasswordHash, user.PasswordSalt))
            return false;

        // V1.0.0 used PBKDF2. After a successful legacy login, immediately
        // upgrade the stored credential to the current Argon2id policy.
        if (PasswordHasher.NeedsRehash(user.PasswordHash))
        {
            var (hash, salt) = PasswordHasher.HashPassword(password);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            await db.SaveChangesAsync();
        }

        AppSession.SignIn(user);
        return true;
    }
}
