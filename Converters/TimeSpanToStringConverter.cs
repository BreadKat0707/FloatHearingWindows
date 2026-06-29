using Microsoft.UI.Xaml.Data;

namespace FloatHearing.Converters;

/// <summary>
/// 将 TimeSpan 格式化为 mm:ss 或 hh:mm:ss 字符串。
/// </summary>
public sealed class TimeSpanToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours >= 1)
            {
                return $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }

            return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
