using System.Diagnostics;

namespace RollOnInjector.Services;

public static class RobloxLauncher
{
    public static void Launch()
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "roblox-player:",
                UseShellExecute = true
            }
        };
        process.Start();
    }
}
