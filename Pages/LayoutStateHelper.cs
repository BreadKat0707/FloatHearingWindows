using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FloatHearing.Pages;

/// <summary>
/// 桌面端与移动端布局状态判断辅助类。
/// </summary>
public static class LayoutStateHelper
{
    public static double LastKnownWindowWidth { get; set; } = 1024;

    public static bool IsCompact(double width)
    {
        return width <= 720;
    }

    public static bool IsNarrow(double width)
    {
        return width <= 540;
    }
}
