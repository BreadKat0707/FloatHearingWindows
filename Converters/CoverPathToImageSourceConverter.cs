using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace FloatHearing.Converters;

/// <summary>
/// 将相对封面路径转换为 BitmapImage。
/// </summary>
public sealed class CoverPathToImageSourceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string coverPath && !string.IsNullOrWhiteSpace(coverPath))
        {
            try
            {
                var uri = new Uri($"ms-appdata:///local/{coverPath.Replace('\\', '/')}");
                var bitmapImage = new BitmapImage();
                bitmapImage.UriSource = uri;
                return bitmapImage;
            }
            catch
            {
                // 忽略加载失败
            }
        }

        return new BitmapImage();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
