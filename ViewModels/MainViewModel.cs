using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RollOnInjector.Models;
using RollOnInjector.Services;

namespace RollOnInjector.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly RobloxLocator _locator = new();
    private readonly BackupService _backup = new();
    private string _searchText = string.Empty;
    private string _status = "Ready";
    private string _currentPath = "Not detected";

    public ObservableCollection<FastFlag> Flags { get; } = new();
    public ObservableCollection<string> Backups { get; } = new();

    public string SearchText
    {
        get => _searchText;
        set { _searchText = value; OnPropertyChanged(); OnPropertyChanged(nameof(FilteredFlags)); }
    }

    public string Status { get => _status; private set { _status = value; OnPropertyChanged(); } }
    public string CurrentPath { get => _currentPath; private set { _currentPath = value; OnPropertyChanged(); } }

    public IEnumerable<FastFlag> FilteredFlags => string.IsNullOrWhiteSpace(SearchText)
        ? Flags
        : Flags.Where(f => f.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || f.Category.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

    public async Task RefreshAsync()
    {
        try
        {
            CurrentPath = _locator.GetClientSettingsPath() ?? "Roblox version not detected";
            Flags.Clear();
            if (CurrentPath != "Roblox version not detected" && File.Exists(CurrentPath))
            {
                var loaded = await FlagDatabase.LoadAsync(CurrentPath);
                foreach (var flag in loaded) Flags.Add(flag);
            }
            RefreshBackups();
            Status = $"Loaded {Flags.Count} configured flag(s)";
        }
        catch (Exception ex)
        {
            Status = $"Load error: {ex.Message}";
        }
        OnPropertyChanged(nameof(FilteredFlags));
    }

    public async Task SaveAsync()
    {
        if (CurrentPath == "Roblox version not detected")
        {
            Status = "Roblox installation not detected.";
            return;
        }

        try
        {
            if (File.Exists(CurrentPath)) _backup.CreateBackup(CurrentPath);
            await FlagDatabase.SaveAsync(CurrentPath, Flags);
            RefreshBackups();
            Status = $"Applied {Flags.Count(f => f.Enabled)} enabled flag(s) to local configuration";
        }
        catch (Exception ex)
        {
            Status = $"Save error: {ex.Message}";
        }
    }

    public void RefreshBackups()
    {
        Backups.Clear();
        foreach (var item in _backup.GetBackups()) Backups.Add(item);
    }

    public async Task ImportAsync(string path)
    {
        try
        {
            var imported = await FlagImportService.ImportAsync(path);
            foreach (var item in imported)
            {
                var existing = Flags.FirstOrDefault(f => f.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase));
                if (existing is null)
                {
                    item.Enabled = true;
                    Flags.Add(item);
                }
                else
                {
                    existing.Value = item.Value;
                    existing.Enabled = true;
                }
            }
            Status = $"Imported {imported.Count} flag(s) into editor";
            OnPropertyChanged(nameof(FilteredFlags));
        }
        catch (Exception ex) { Status = $"Import error: {ex.Message}"; }
    }

    public void ResetAll()
    {
        foreach (var flag in Flags) flag.Enabled = false;
        Status = "Editor reset. Nothing was saved yet.";
    }

    public void ApplyPreset(string preset)
    {
        foreach (var flag in Flags) flag.Enabled = false;

        string[] keywords = preset switch
        {
            "Competitive" => new[] { "Graphics", "Texture", "Render", "Frame", "Debug" },
            "Low-end" => new[] { "Texture", "Graphics", "Render", "Quality" },
            _ => new[] { "Graphics", "Network" }
        };

        foreach (var flag in Flags)
        {
            if (keywords.Any(k => flag.Name.Contains(k, StringComparison.OrdinalIgnoreCase)))
                flag.Enabled = true;
        }

        Status = $"Loaded {preset} profile into editor";
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
