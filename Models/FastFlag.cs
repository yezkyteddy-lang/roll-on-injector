using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RollOnInjector.Models;

public sealed class FastFlag : INotifyPropertyChanged
{
    private string _value;
    private bool _enabled;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = "General";

    public string Value
    {
        get => _value;
        set { _value = value; OnPropertyChanged(); }
    }

    public bool Enabled
    {
        get => _enabled;
        set { _enabled = value; OnPropertyChanged(); }
    }

    public FastFlag() : this(string.Empty, string.Empty) { }

    public FastFlag(string name, string value, string description = "", string category = "General")
    {
        Name = name;
        _value = value;
        Description = description;
        Category = category;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
