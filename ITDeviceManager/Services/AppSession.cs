using ITDeviceManager.Models;

namespace ITDeviceManager.Services;

public static class AppSession
{
    public static User? CurrentUser { get; private set; }
    public static bool IsAdmin => CurrentUser?.Role?.Name == "Admin";

    public static void SignIn(User user)
    {
        CurrentUser = user;
        PermissionService.Reset();
    }

    public static void SignOut()
    {
        CurrentUser = null;
        PermissionService.Reset();
    }
}
