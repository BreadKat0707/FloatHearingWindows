using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Controls;

namespace FloatHearing.Converters;

/// <summary>
/// 将布尔值转换为播放/暂停图标符号：true -> Pause，false -> Play。
/// </summary>
public sealed class BoolToPlayPauseSymbolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is true)
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
