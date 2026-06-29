using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace FloatHearing.Converters;

/// <summary>
/// 根据是否窄屏返回不同内边距。
/// </summary>
public sealed class NarrowToPaddingConverter : IValueConverter
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
/// 根据是否窄屏返回不同按钮间距。
/// </summary>
public sealed class NarrowToSpacingConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is true ? 8.0 : 12.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 根据是否窄屏返回不同音量滑块宽度。
/// </summary>
public sealed class NarrowToVolumeWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is true ? 80.0 : 120.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
