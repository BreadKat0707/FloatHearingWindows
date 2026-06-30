using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
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

    private void SongsListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        if (e.OriginalSource is FrameworkElement element && element.DataContext is Song song)
        {
            ViewModel.ContextMenu.SetContext(song, ViewModel.Songs.ToList());
            BuildAddToPlaylistSubMenu();
            var flyout = Resources["SongContextMenuFlyout"] as MenuFlyout;
            flyout?.ShowAt(element, e.GetPosition(element));
            e.Handled = true;
        }
    }

    private void BuildAddToPlaylistSubMenu()
    {
        if (AddToPlaylistSubMenu is null)
        {
            return;
        }

        AddToPlaylistSubMenu.Items.Clear();
        foreach (var playlist in ViewModel.ContextMenu.Playlists)
        {
            var item = new MenuFlyoutItem
            {
                Text = playlist.Name,
                Tag = playlist.Id,
                FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Microsoft YaHei UI")
            };
            item.Click += AddToPlaylistSubMenuItem_Click;
            AddToPlaylistSubMenu.Items.Add(item);
        }
    }

    private void PlayNowMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ContextMenu.PlayNow();
    }

    private void PlayNextMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ContextMenu.PlayNext();
    }

    private void ViewAlbumMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var album = ViewModel.ContextMenu.TargetSong?.Album;
        if (string.IsNullOrWhiteSpace(album))
        {
            return;
        }

        // TODO: 导航到专辑页并筛选该专辑
    }

    private void ViewArtistMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var artist = ViewModel.ContextMenu.TargetSong?.Artist;
        if (string.IsNullOrWhiteSpace(artist))
        {
            return;
        }

        // TODO: 导航到艺术家页并筛选该艺术家
    }

    private async void OpenContainingFolderMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var path = ViewModel.ContextMenu.TargetSong?.FilePath;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return;
        }

        var folder = Path.GetDirectoryName(path);
        if (string.IsNullOrWhiteSpace(folder))
        {
            return;
        }

        var folderItem = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(folder);
        await Launcher.LaunchFolderAsync(folderItem);
    }

    private async void OpenWithExternalAppMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var path = ViewModel.ContextMenu.TargetSong?.FilePath;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return;
        }

        var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(path);
        await Launcher.LaunchFileAsync(file);
    }

    private void CopySongNameMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var name = ViewModel.ContextMenu.TargetSong?.Title;
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        var package = new DataPackage();
        package.SetText(name);
        Clipboard.SetContent(package);
    }

    private void CopyFilePathMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var path = ViewModel.ContextMenu.TargetSong?.FilePath;
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        var package = new DataPackage();
        package.SetText(path);
        Clipboard.SetContent(package);
    }

    private async void PropertiesMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var song = ViewModel.ContextMenu.TargetSong;
        if (song is null)
        {
            return;
        }

        var dialog = new ContentDialog
        {
            Title = "音乐标签与文件属性",
            Content = new ScrollViewer
            {
                Content = new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        new TextBlock { Text = $"标题: {song.Title}" },
                        new TextBlock { Text = $"艺术家: {song.Artist}" },
                        new TextBlock { Text = $"专辑: {song.Album}" },
                        new TextBlock { Text = $"专辑艺术家: {song.AlbumArtist}" },
                        new TextBlock { Text = $"时长: {song.Duration:g}" },
                        new TextBlock { Text = $"文件大小: {song.FileSize} 字节" },
                        new TextBlock { Text = $"路径: {song.FilePath}" },
                        new TextBlock { Text = $"添加时间: {song.DateAdded:yyyy-MM-dd HH:mm}" },
                        new TextBlock { Text = $"播放次数: {song.PlayCount}" }
                    }
                }
            },
            CloseButtonText = "确定",
            XamlRoot = XamlRoot
        };

        await dialog.ShowAsync();
    }

    private async void RescanMetadataMenuItem_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.ContextMenu.RescanMetadataAsync();
        await ViewModel.LoadLibraryAsync();
    }

    private async void RemoveFromLibraryMenuItem_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.ContextMenu.RemoveFromLibraryAsync();
        await ViewModel.LoadLibraryAsync();
    }

    private async void DeleteFromDiskMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var song = ViewModel.ContextMenu.TargetSong;
        if (song is null)
        {
            return;
        }

        var dialog = new ContentDialog
        {
            Title = "确认删除",
            Content = $"确定要从本地磁盘删除文件吗？\n{song.FilePath}",
            PrimaryButtonText = "删除",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            return;
        }

        try
        {
            if (File.Exists(song.FilePath))
            {
                File.Delete(song.FilePath);
            }
        }
        catch (Exception ex)
        {
            var errorDialog = new ContentDialog
            {
                Title = "删除失败",
                Content = ex.Message,
                CloseButtonText = "确定",
                XamlRoot = XamlRoot
            };
            await errorDialog.ShowAsync();
            return;
        }

        await ViewModel.ContextMenu.RemoveFromLibraryAsync();
        await ViewModel.LoadLibraryAsync();
    }

    private void SearchSameNameMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var song = ViewModel.ContextMenu.TargetSong;
        if (song is null)
        {
            return;
        }

        var query = $"\"{song.Title}\"";
        _ = Launcher.LaunchUriAsync(new Uri($"https://www.bing.com/search?q={Uri.EscapeDataString(query)}"));
    }

    private async void AddToPlaylistSubMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.Tag is long playlistId)
        {
            await ViewModel.ContextMenu.AddToPlaylistAsync(playlistId);
        }
    }
}
