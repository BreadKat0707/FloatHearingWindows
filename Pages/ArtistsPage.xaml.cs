using Microsoft.UI.Xaml.Controls;
using FloatHearing.Models;
using FloatHearing.ViewModels;

namespace FloatHearing.Pages;

/// <summary>
/// 艺术家页面
/// </summary>
public sealed partial class ArtistsPage : Page
{
    public ArtistsViewModel ViewModel { get; }

    public ArtistsPage()
    {
        ViewModel = new ArtistsViewModel(App.DbContext);
        InitializeComponent();
        Loaded += ArtistsPage_Loaded;
    }

    private async void ArtistsPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await ViewModel.LoadArtistsAsync();
    }

    private void ArtistsListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Artist artist)
        {
            Frame.Navigate(typeof(ArtistDetailPage), artist);
        }
    }
}
