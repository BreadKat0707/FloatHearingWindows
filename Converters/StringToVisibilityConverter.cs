using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace FloatHearing.Converters;

/// <summary>
/// 判断字符串是否为空，用于封面路径的 Visibility 切换。
/// true（有路径）- Visible，false（无路径）- Collapsed。
/// </summary>
public sealed class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var invert = parameter is string param && param.Equals("Invert", StringComparison.OrdinalIgnoreCase);
        var hasValue = value is string str && !string.IsNullOrWhiteSpace(str);
        var result = invert ? !hasValue : hasValue;
        return result ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
