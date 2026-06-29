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
                var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
                var fullPath = Path.Combine(localFolder, coverPath);
                if (File.Exists(fullPath))
                {
                    var bitmapImage = new BitmapImage();
                    bitmapImage.UriSource = new Uri(fullPath);
                    return bitmapImage;
                }
            }
            catch
            {
                // 忽略加载失败
            }
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
