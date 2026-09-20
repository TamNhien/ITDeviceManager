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

        if (user is null || !PasswordHasher.Verify(password, user.PasswordHash))
            return false;

        // Rehash transparently when Argon2id cost parameters are strengthened.
        if (PasswordHasher.NeedsRehash(user.PasswordHash))
        {
            user.PasswordHash = PasswordHasher.HashPassword(password);
            await db.SaveChangesAsync();
        }

        AppSession.SignIn(user);
        return true;
    }
}
