namespace RollOnInjector.Services;

public sealed class RobloxLocator
{
    public string VersionsRoot => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Roblox", "Versions");

    public string? FindLatestVersionDirectory()
    {
        if (!Directory.Exists(VersionsRoot)) return null;

        var candidates = Directory.EnumerateDirectories(VersionsRoot)
            .Where(d => File.Exists(Path.Combine(d, "RobloxPlayerBeta.exe")) ||
                        File.Exists(Path.Combine(d, "RobloxPlayerLauncher.exe")))
            .Select(d => new DirectoryInfo(d))
            .OrderByDescending(d => d.LastWriteTimeUtc)
            .ToList();

        return candidates.FirstOrDefault()?.FullName;
    }

    public string? GetClientSettingsPath()
    {
        var version = FindLatestVersionDirectory();
        if (version is null) return null;
        var directory = Path.Combine(version, "ClientSettings");
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, "ClientAppSettings.json");
    }
}
