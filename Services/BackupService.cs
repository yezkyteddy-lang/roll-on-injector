namespace RollOnInjector.Services;

public sealed class BackupService
{
    private readonly string _backupRoot = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "RollOnInjector", "Backups");

    public string CreateBackup(string? settingsPath)
    {
        if (string.IsNullOrWhiteSpace(settingsPath) || !File.Exists(settingsPath))
            throw new FileNotFoundException("ClientAppSettings.json was not found.", settingsPath);

        Directory.CreateDirectory(_backupRoot);
        var filename = $"ClientAppSettings_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        var destination = Path.Combine(_backupRoot, filename);
        File.Copy(settingsPath, destination, overwrite: false);
        return destination;
    }

    public IReadOnlyList<string> GetBackups() =>
        Directory.Exists(_backupRoot)
            ? Directory.EnumerateFiles(_backupRoot, "*.json").OrderByDescending(File.GetLastWriteTimeUtc).ToArray()
            : Array.Empty<string>();

    public void Restore(string backupPath, string settingsPath)
    {
        if (!File.Exists(backupPath)) throw new FileNotFoundException("Backup not found.", backupPath);
        Directory.CreateDirectory(Path.GetDirectoryName(settingsPath)!);
        File.Copy(backupPath, settingsPath, overwrite: true);
    }
}
