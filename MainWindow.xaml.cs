using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using RollOnInjector.Services;
using RollOnInjector.ViewModels;

namespace RollOnInjector;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm = new();
    private bool _bootFinished;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _vm;
        _vm.PropertyChanged += ViewModel_PropertyChanged;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var progress = (Storyboard)FindResource("BootProgressStoryboard");
        progress.Begin(this);

        await _vm.RefreshAsync();
        UpdateUi();

        await Task.Delay(1250);
        BootStatus.Text = "Workspace ready";
        await Task.Delay(250);

        var fade = (Storyboard)FindResource("BootFadeStoryboard");
        fade.Completed += (_, _) =>
        {
            LoadingOverlay.IsHitTestVisible = false;
            LoadingOverlay.Visibility = Visibility.Collapsed;
            _bootFinished = true;
        };
        fade.Begin(this);
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Dispatcher.Invoke(UpdateUi);
    }

    private void UpdateUi()
    {
        var detected = !string.Equals(_vm.CurrentPath, "Roblox version not detected", StringComparison.OrdinalIgnoreCase)
                       && !string.Equals(_vm.CurrentPath, "Not detected", StringComparison.OrdinalIgnoreCase);
        RobloxStatusText.Text = detected ? "Detected" : "Not found";
        PathText.Text = _vm.CurrentPath;
        CurrentPathText.Text = _vm.CurrentPath;
        FlagCountText.Text = _vm.Flags.Count(f => f.Enabled).ToString();
        BackupCountText.Text = _vm.Backups.Count.ToString();
        StatusText.Text = _vm.Status;
        SidebarVersion.Text = detected ? "Roblox: detected" : "Roblox: not detected";
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void Maximize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    private void Window_StateChanged(object? sender, EventArgs e) { }

    private void Dashboard_Click(object sender, RoutedEventArgs e) => SetPage("Dashboard", "A cleaner, faster control center for your local Fast Flag setup.");
    private void Flags_Click(object sender, RoutedEventArgs e) => SetPage("Fast Flags", "Search, edit and enable local client configuration flags.");
    private void Presets_Click(object sender, RoutedEventArgs e) => SetPage("Presets", "Load a profile, then tune individual settings before applying.");
    private void Settings_Click(object sender, RoutedEventArgs e) => SetPage("Settings", "Roblox detection, backups and local application behavior.");

    private void SetPage(string title, string subtitle)
    {
        PageTitle.Text = title;
        PageSubtitle.Text = subtitle;
        StatusText.Text = $"Section: {title}";
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        await _vm.SaveAsync();
        UpdateUi();
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        await _vm.RefreshAsync();
        UpdateUi();
    }

    private async void Import_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            Title = "Import Fast Flag JSON"
        };
        if (dialog.ShowDialog() == true)
        {
            await _vm.ImportAsync(dialog.FileName);
            UpdateUi();
        }
    }

    private void Launch_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            RobloxLauncher.Launch();
            StatusText.Text = "Roblox launch requested.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Launch failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        _vm.ResetAll();
        UpdateUi();
    }

    private void PresetBalanced_Click(object sender, RoutedEventArgs e)
    {
        _vm.ApplyPreset("Balanced");
        ProfileText.Text = "Balanced";
        UpdateUi();
    }

    private void PresetCompetitive_Click(object sender, RoutedEventArgs e)
    {
        _vm.ApplyPreset("Competitive");
        ProfileText.Text = "Competitive";
        UpdateUi();
    }

    private void PresetLowEnd_Click(object sender, RoutedEventArgs e)
    {
        _vm.ApplyPreset("Low-end");
        ProfileText.Text = "Low-end";
        UpdateUi();
    }

    private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        _vm.SearchText = SearchBox.Text;
        FlagGrid.ItemsSource = _vm.FilteredFlags;
    }
}
