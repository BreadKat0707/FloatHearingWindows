using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using FloatHearing.ViewModels;

namespace FloatHearing.Pages;

/// <summary>
/// 扫描目录管理页面
/// </summary>
public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }

    public SettingsPage()
    {
        ViewModel = new SettingsViewModel(App.DbContext);
        InitializeComponent();
        Loaded += SettingsPage_Loaded;
        Unloaded += SettingsPage_Unloaded;
        SizeChanged += SettingsPage_SizeChanged;
    }

    private void SettingsPage_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        LayoutStateHelper.LastKnownWindowWidth = e.NewSize.Width;
        ApplyNarrowLayout(LayoutStateHelper.IsNarrow(e.NewSize.Width));
    }

    private async void SettingsPage_Loaded(object sender, RoutedEventArgs e)
    {
        LayoutStateHelper.LastKnownWindowWidth = ActualSize.X;
        ApplyNarrowLayout(LayoutStateHelper.IsNarrow(ActualSize.X));
        await ViewModel.LoadScanPathsAsync();
    }

    private void SettingsPage_Unloaded(object sender, RoutedEventArgs e)
    {
    }

    private void ApplyNarrowLayout(bool narrow)
    {
        if (RootGrid is not null)
        {
            RootGrid.Padding = narrow
                ? new Thickness(12, 8, 12, 8)
                : new Thickness(24, 16, 24, 16);
        }

        if (PageTitle is not null)
        {
            PageTitle.FontSize = narrow ? 20 : 24;
        }

        if (ToolbarRight is not null)
        {
            ToolbarRight.Spacing = narrow ? 6 : 8;
        }

        if (ScanProgressPanel is not null)
        {
            ScanProgressPanel.Spacing = narrow ? 6 : 8;
        }
    }

    private async void AddFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var folderPicker = new FolderPicker
        {
            SuggestedStartLocation = PickerLocationId.MusicLibrary,
            ViewMode = PickerViewMode.List
        };
        folderPicker.FileTypeFilter.Add("*");

        if (App.MainWindow is not null)
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);
        }

        var folder = await folderPicker.PickSingleFolderAsync();
        if (folder is null)
        {
            return;
        }

        await ViewModel.AddScanPathAsync(folder.Path);
    }

    private async void ScanAllButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.ScanAllAsync();
    }

    private async void DeleteScanPathButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is long id)
        {
            await ViewModel.RemoveScanPathAsync(id);
        }
    }
}
