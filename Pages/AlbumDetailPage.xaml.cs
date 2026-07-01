using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using FloatHearing.Models;
using FloatHearing.ViewModels;

namespace FloatHearing.Pages;

/// <summary>
/// 专辑详情页
/// </summary>
public sealed partial class AlbumDetailPage : Page
{
    public AlbumDetailViewModel? ViewModel { get; private set; }

    public AlbumDetailPage()
    {
        ViewModel = new AlbumDetailViewModel(App.DbContext);
        DataContext = ViewModel;
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is Album album && ViewModel is not null)
        {
            _ = ViewModel.InitializeAsync(album);
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
}
