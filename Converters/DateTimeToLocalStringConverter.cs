using Microsoft.UI.Xaml.Data;

namespace FloatHearing.Converters;

/// <summary>
/// 将 UTC DateTime 转换为本地时间字符串。
/// </summary>
public sealed class DateTimeToLocalStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTime dateTime)
        {
            var local = dateTime.Kind == DateTimeKind.Utc
                ? dateTime.ToLocalTime()
                : dateTime;
            return local.ToString("yyyy-MM-dd HH:mm");
        }

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
