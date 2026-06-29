using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using FloatHearing.Models;
using FloatHearing.ViewModels;

namespace FloatHearing.Pages;

/// <summary>
/// 歌曲页面
/// </summary>
public sealed partial class SongsPage : Page
{
    public SongsViewModel ViewModel { get; }

    public SongsPage()
    {
        ViewModel = new SongsViewModel(App.PlaybackService);
        InitializeComponent();
        Loaded += SongsPage_Loaded;
        Unloaded += SongsPage_Unloaded;
        SizeChanged += SongsPage_SizeChanged;
    }

    private void SongsPage_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        LayoutStateHelper.LastKnownWindowWidth = e.NewSize.Width;
        ApplyNarrowLayout(LayoutStateHelper.IsNarrow(e.NewSize.Width));
    }

    private async void SongsPage_Loaded(object sender, RoutedEventArgs e)
    {
        LayoutStateHelper.LastKnownWindowWidth = ActualSize.X;
        ApplyNarrowLayout(LayoutStateHelper.IsNarrow(ActualSize.X));
        await ViewModel.LoadLibraryAsync();
        await ViewModel.ScanAsync();
    }

    private void SongsPage_Unloaded(object sender, RoutedEventArgs e)
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

        if (SortFieldComboBox is not null)
        {
            SortFieldComboBox.Width = narrow ? 100 : double.NaN;
        }
    }

    private void SortDirectionButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ToggleSortDirection();
    }

    private void SongsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SongsListView.SelectedItem is Song song)
        {
            ViewModel.SelectedSong = song;
        }
    }

    private void SongsListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Song song)
        {
            SongsListView.SelectedItem = song;
        }
    }
}
