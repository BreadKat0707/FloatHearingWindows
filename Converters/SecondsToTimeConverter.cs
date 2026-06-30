using System;
using Microsoft.UI.Xaml.Data;

namespace FloatHearing.Converters;

/// <summary>
/// 将秒数转换为 mm:ss 显示格式，用于播放进度滑块提示。
/// </summary>
public sealed class SecondsToTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double seconds)
        {
            var span = TimeSpan.FromSeconds(seconds);
            return $"{span.Minutes:D2}:{span.Seconds:D2}";
        }

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
