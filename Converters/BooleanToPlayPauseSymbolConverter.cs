using Microsoft.UI.Xaml.Controls;

namespace FloatHearing.Converters;

/// <summary>
/// 根据布尔值返回播放或暂停符号图标。
/// </summary>
public sealed class BooleanToPlayPauseSymbolConverter : Microsoft.UI.Xaml.Data.IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isPlaying && isPlaying)
        {
            return Symbol.Pause;
        }

        return Symbol.Play;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
