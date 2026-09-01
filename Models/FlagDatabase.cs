using System.Text.Json;

namespace RollOnInjector.Models;

public static class FlagDatabase
{
    public static async Task<List<FastFlag>> LoadAsync(string path)
    {
        if (!File.Exists(path)) return new();
        await using var stream = File.OpenRead(path);
        var map = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(stream)
                  ?? new Dictionary<string, string>();
        return map.Select(kvp => new FastFlag(kvp.Key, kvp.Value, category: GuessCategory(kvp.Key))).ToList();
    }

    public static async Task SaveAsync(string path, IEnumerable<FastFlag> flags)
    {
        var map = flags.Where(f => f.Enabled && !string.IsNullOrWhiteSpace(f.Name))
                       .ToDictionary(f => f.Name, f => f.Value);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var json = JsonSerializer.Serialize(map, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(path, json);
    }

    private static string GuessCategory(string name)
    {
        if (name.Contains("Texture", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Graphics", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Render", StringComparison.OrdinalIgnoreCase)) return "Graphics";
        if (name.Contains("Network", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Rak", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Ping", StringComparison.OrdinalIgnoreCase)) return "Network";
        if (name.Contains("Telemetry", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Analytics", StringComparison.OrdinalIgnoreCase)) return "Telemetry";
        return "General";
    }
}
