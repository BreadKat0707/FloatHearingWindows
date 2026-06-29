using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using FloatHearing.Pages;
using FloatHearing.Services;
using FloatHearing.ViewModels;

namespace FloatHearing;

/// <summary>
/// 应用主导航外壳，包含侧边栏、内容 Frame 与底部播放控制栏。
/// </summary>
public sealed partial class MainPage : Page
{
    public ShellViewModel ViewModel { get; }

    public MainPage()
    {
        ViewModel = new ShellViewModel(App.PlaybackService);
        InitializeComponent();
        Loaded += MainPage_Loaded;
        Unloaded += MainPage_Unloaded;
        SizeChanged += MainPage_SizeChanged;
    }

    private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateLayoutState(e.NewSize.Width);
    }

    private void UpdateLayoutState(double width)
    {
        LayoutStateHelper.LastKnownWindowWidth = width;
        var isCompact = LayoutStateHelper.IsCompact(width);

        if (RootNavigationView is not null)
        {
            RootNavigationView.PaneDisplayMode = isCompact
                ? NavigationViewPaneDisplayMode.LeftCompact
                : NavigationViewPaneDisplayMode.Left;
        }

        ApplyPlaybackBarLayout(LayoutStateHelper.IsNarrow(width));
    }

    private void ApplyPlaybackBarLayout(bool narrow)
    {
        if (PlaybackBar is null)
        {
            return;
        }

        PlaybackBar.Padding = narrow
            ? new Thickness(12, 8, 12, 8)
            : new Thickness(24, 16, 24, 16);

        if (PlaybackButtons is not null)
        {
            PlaybackButtons.Spacing = narrow ? 8 : 12;
        }

        if (VolumeSlider is not null)
        {
            VolumeSlider.Width = narrow ? 80 : 120;
        }
    }

    private void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateLayoutState(ActualSize.X);
        RootNavigationView.SelectedItem = SongsNavItem;
        ContentFrame.Navigate(typeof(SongsPage));

        App.PlaybackService.PropertyChanged += PlaybackService_PropertyChanged;
        UpdatePlaybackButtonSymbol();
        UpdateCurrentSongDisplay();
        UpdatePositionSlider();
    }

    private void MainPage_Unloaded(object sender, RoutedEventArgs e)
    {
        App.PlaybackService.PropertyChanged -= PlaybackService_PropertyChanged;
    }

    private void PlaybackService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PlaybackService.IsPlaying))
        {
            UpdatePlaybackButtonSymbol();
        }
        else if (e.PropertyName == nameof(PlaybackService.CurrentSong))
        {
            UpdateCurrentSongDisplay();
            UpdatePositionSlider();
        }
        else if (e.PropertyName == nameof(PlaybackService.Duration))
        {
            UpdatePositionSlider();
        }
        else if (e.PropertyName == nameof(PlaybackService.CurrentPosition))
        {
            if (PositionSlider is not null && Math.Abs(PositionSlider.Value - App.PlaybackService.CurrentPosition) > 1.0)
            {
                PositionSlider.Value = App.PlaybackService.CurrentPosition;
            }
        }
    }

    private void UpdatePlaybackButtonSymbol()
    {
        if (PlayPauseSymbol is null)
        {
            return;
        }

        PlayPauseSymbol.Symbol = App.PlaybackService.IsPlaying ? Symbol.Pause : Symbol.Play;
    }

    private void UpdateCurrentSongDisplay()
    {
        var song = App.PlaybackService.CurrentSong;
        if (CurrentTitleText is not null)
        {
            CurrentTitleText.Text = song?.Title ?? "未在播放";
        }

        if (CurrentArtistText is not null)
        {
            CurrentArtistText.Text = song?.Artist ?? "选择一首歌曲开始播放";
        }
    }

    private void UpdatePositionSlider()
    {
        if (PositionSlider is null)
        {
            return;
        }

        PositionSlider.Maximum = App.PlaybackService.Duration > 0
            ? App.PlaybackService.Duration
            : 0;
    }

    private void PositionSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (App.PlaybackService.Duration > 0)
        {
            App.PlaybackService.CurrentPosition = e.NewValue;
        }
    }

    private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            ContentFrame.Navigate(typeof(SettingsPage), null, args.RecommendedNavigationTransitionInfo);
            return;
        }

        if (args.SelectedItem is NavigationViewItem item && item.Tag is string tag)
        {
            var pageType = tag switch
            {
                "Songs" => typeof(SongsPage),
                "Albums" => typeof(AlbumsPage),
                "Artists" => typeof(ArtistsPage),
                "Playlists" => typeof(PlaylistsPage),
                "Ideas" => typeof(IdeasPage),
                "Stats" => typeof(StatsPage),
                "ScanFolders" => typeof(ScanFoldersPage),
                _ => typeof(SongsPage)
            };

            ContentFrame.Navigate(pageType, null, args.RecommendedNavigationTransitionInfo);
        }
    }

    private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
    {
        App.PlaybackService.PlayPause();
    }

    private void PreviousButton_Click(object sender, RoutedEventArgs e)
    {
        App.PlaybackService.Previous();
    }

    private void NextButton_Click(object sender, RoutedEventArgs e)
    {
        App.PlaybackService.Next();
    }
}
