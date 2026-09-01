using System.Text.Json;
using RollOnInjector.Models;

namespace RollOnInjector.Services;

public static class FlagImportService
{
    public static async Task<List<FastFlag>> ImportAsync(string path)
    {
        await using var stream = File.OpenRead(path);
        var root = await JsonSerializer.DeserializeAsync<JsonElement>(stream);
        var output = new List<FastFlag>();

        if (root.ValueKind != JsonValueKind.Object) return output;

        foreach (var property in root.EnumerateObject())
        {
            string value = property.Value.ValueKind switch
            {
                JsonValueKind.String => property.Value.GetString() ?? string.Empty,
                JsonValueKind.Number => property.Value.ToString(),
                JsonValueKind.True => "True",
                JsonValueKind.False => "False",
                _ => property.Value.ToString()
            };
            output.Add(new FastFlag(property.Name, value, category: "Imported"));
        }

        return output;
    }
}
