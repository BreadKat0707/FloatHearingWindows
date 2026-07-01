using Microsoft.UI.Xaml.Controls;
using FloatHearing.Models;
using FloatHearing.ViewModels;

namespace FloatHearing.Pages;

/// <summary>
/// 专辑页面
/// </summary>
public sealed partial class AlbumsPage : Page
{
    public AlbumsViewModel ViewModel { get; }

    public AlbumsPage()
    {
        ViewModel = new AlbumsViewModel(App.DbContext);
        InitializeComponent();
        Loaded += AlbumsPage_Loaded;
    }

    private void AlbumsGridView_ItemClick(object sender, Microsoft.UI.Xaml.Controls.ItemClickEventArgs e)
    {
        if (e.ClickedItem is Album album)
        {
            Frame.Navigate(typeof(AlbumDetailPage), album);
        }
    }

    private async void AlbumsPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await ViewModel.LoadAlbumsAsync();
    }
}
