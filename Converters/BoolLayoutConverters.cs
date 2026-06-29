using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace FloatHearing.Converters;

/// <summary>
/// 根据布尔值返回不同内边距。
/// </summary>
public sealed class BoolToThicknessConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is true)
        {
            return new Thickness(12, 8, 12, 8);
        }

        return new Thickness(24, 16, 24, 16);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 根据布尔值返回不同标题字号。
/// </summary>
public sealed class BoolToTitleFontSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is true ? 20.0 : 24.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 根据布尔值返回不同间距。
/// </summary>
public sealed class BoolToSpacingConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is true ? 6.0 : 8.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 根据布尔值返回排序下拉框宽度。
/// </summary>
public sealed class BoolToSortComboWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is true ? 100.0 : double.NaN;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
