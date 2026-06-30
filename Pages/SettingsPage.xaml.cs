using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using FloatHearing.Data.Entities;
using FloatHearing.Services;
using FloatHearing.ViewModels;

namespace FloatHearing.Pages;

/// <summary>
/// 设置页。
/// </summary>
public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }

    public SettingsPage()
    {
        ViewModel = new SettingsViewModel();
        InitializeComponent();

        Loaded += SettingsPage_Loaded;
    }

    private async void SettingsPage_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadAsync();
        ApplyTheme();
        ApplyBackdrop();
    }

    private void ThemeModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyTheme();
    }

    private void BackdropMaterialComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyBackdrop();
    }

    private void ApplyTheme()
    {
        var theme = ViewModel.SelectedThemeModeOption.Mode switch
        {
            ThemeMode.Light => ElementTheme.Light,
            ThemeMode.Dark => ElementTheme.Dark,
            _ => ElementTheme.Default
        };

        var window = App.MainWindow;
        if (window?.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = theme;
        }
    }

    private void ApplyBackdrop()
    {
        var window = App.MainWindow;
        if (window is null)
        {
            return;
        }

        if (window.SystemBackdrop is IDisposable disposable)
        {
            disposable.Dispose();
        }

        window.SystemBackdrop = ViewModel.SelectedBackdropMaterialOption.Material switch
        {
            BackdropMaterial.Acrylic => new Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop(),
            BackdropMaterial.Mica => new Microsoft.UI.Xaml.Media.MicaBackdrop(),
            BackdropMaterial.MicaAlt => new Microsoft.UI.Xaml.Media.MicaBackdrop { Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt },
            _ => null
        };
    }
}
