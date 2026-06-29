using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using FloatHearing.Pages;

namespace FloatHearing.Converters;

/// <summary>
/// 根据当前窗口窄屏状态返回歌曲列表项的布局参数。
/// </summary>
public sealed class SongItemLayoutConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        try
        {
            var param = parameter as string;
            var narrow = LayoutStateHelper.IsNarrow(LayoutStateHelper.LastKnownWindowWidth);

            return param switch
            {
                "Padding" => narrow ? new Thickness(6, 4, 6, 4) : new Thickness(8, 6, 8, 6),
                "CoverSize" => narrow ? 40.0 : 48.0,
                "CoverMargin" => narrow ? 8.0 : 12.0,
                "IconFontSize" => narrow ? 20.0 : 24.0,
                "TitleFontSize" => narrow ? 13.0 : 14.0,
                "SubtitleFontSize" => narrow ? 11.0 : 12.0,
                "DurationFontSize" => narrow ? 11.0 : 12.0,
                _ => 0.0
            };
        }
        catch
        {
            var fallbackParam = parameter as string;
            return fallbackParam switch
            {
                "Padding" => new Thickness(8, 6, 8, 6),
                "CoverSize" => 48.0,
                "CoverMargin" => 12.0,
                "IconFontSize" => 24.0,
                "TitleFontSize" => 14.0,
                "SubtitleFontSize" => 12.0,
                "DurationFontSize" => 12.0,
                _ => 0.0
            };
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
