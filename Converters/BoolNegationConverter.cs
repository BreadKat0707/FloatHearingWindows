using Microsoft.UI.Xaml.Data;

namespace FloatHearing.Converters;

/// <summary>
/// 布尔值取反转换器
/// </summary>
public sealed class BoolNegationConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is false;
    }
}
