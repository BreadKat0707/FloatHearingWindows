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

    private bool _isCompact;
    public bool IsCompact
    {
        get => _isCompact;
        set
        {
            _isCompact = value;
            OnPropertyChanged(nameof(IsCompact));
        }
    }

    private bool _isNarrow;
    public bool IsNarrow
    {
        get => _isNarrow;
        set
        {
            _isNarrow = value;
            OnPropertyChanged(nameof(IsNarrow));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

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
        IsCompact = LayoutStateHelper.IsCompact(width);
        IsNarrow = LayoutStateHelper.IsNarrow(width);

        if (RootNavigationView is not null)
        {
            RootNavigationView.PaneDisplayMode = IsCompact
                ? NavigationViewPaneDisplayMode.LeftCompact
                : NavigationViewPaneDisplayMode.Left;
        }

        ApplyPlaybackBarLayout(IsNarrow);
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
        }
    }

    private void UpdatePlaybackButtonSymbol()
    {
        if (PlayPauseButton is null)
        {
            return;
        }

        var symbol = App.PlaybackService.IsPlaying ? Symbol.Pause : Symbol.Play;
        PlayPauseButton.Content = new SymbolIcon(symbol);
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

    private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
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
                "Settings" => typeof(SettingsPage),
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
