using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using FloatHearing.Models;
using FloatHearing.ViewModels;

namespace FloatHearing.Pages;

/// <summary>
/// 艺术家详情页
/// </summary>
public sealed partial class ArtistDetailPage : Page
{
    public ArtistDetailViewModel? ViewModel { get; private set; }

    public ArtistDetailPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is Artist artist)
        {
            ViewModel = new ArtistDetailViewModel(App.DbContext, artist);
            DataContext = ViewModel;
            _ = ViewModel.LoadAsync();
        }
    }

    private void BackButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }

    private void SongsListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Song song && ViewModel is not null)
        {
            App.PlaybackService.SetQueue(ViewModel.Songs);
            App.PlaybackService.Play(song);
        }
    }

    private void AlbumsGridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Album album)
        {
            Frame.Navigate(typeof(AlbumDetailPage), album);
        }
    }

    private void FeaturedAlbumsGridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Album album)
        {
            Frame.Navigate(typeof(AlbumDetailPage), album);
        }
    }
}
