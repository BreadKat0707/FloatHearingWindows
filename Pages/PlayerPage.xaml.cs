using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using FloatHearing.Services;

namespace FloatHearing.Pages;

/// <summary>
/// 全屏播放器页面
/// </summary>
public sealed partial class PlayerPage : Page
{
    public PlayerPage()
    {
        InitializeComponent();
        DataContext = App.PlaybackService;
    }

    public static event EventHandler? CloseRequested;

    private void BackButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private void PlayPauseButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        App.PlaybackService.PlayPause();
    }

    private void PreviousButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        App.PlaybackService.Previous();
    }

    private void NextButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        App.PlaybackService.Next();
    }

    private void PositionSlider_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
    {
        // Slider 使用 TwoWay 绑定，拖动后自动更新播放位置
    }
}
