using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace FloatHearing.Converters;

/// <summary>
/// 布尔值与 Visibility 转换器：true -> Visible，false -> Collapsed。
/// </summary>
public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (parameter is string param && param.Equals("Invert", StringComparison.OrdinalIgnoreCase))
        {
            return value is false ? Visibility.Visible : Visibility.Collapsed;
        }

        return value is true ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
