using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.UI.Dispatching;

namespace FloatHearing.Services;

/// <summary>
/// 任务栏缩略图工具栏按钮服务，在任务栏应用预览缩略图上添加播放控制按钮。
/// </summary>
public sealed class TaskbarThumbnailButtonService : IDisposable
{
    private const int WmCommand = 0x0111;
    private const uint ThumbnailButtonPrevious = 100;
    private const uint ThumbnailButtonPlayPause = 101;
    private const uint ThumbnailButtonNext = 102;

    private readonly ITaskbarList3 _taskbarList;
    private readonly SubclassProc _subclassProc;
    private IntPtr _hwnd = IntPtr.Zero;
    private readonly List<IntPtr> _iconHandles = [];

    [ComImport]
    [Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface ITaskbarList3
    {
        void HrInit();
        void AddTab(IntPtr hwnd);
        void DeleteTab(IntPtr hwnd);
        void ActivateTab(IntPtr hwnd);
        void SetActiveAlt(IntPtr hwnd);
        void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);
        void SetProgressValue(IntPtr hwnd, ulong ullCompleted, ulong ullTotal);
        void SetProgressState(IntPtr hwnd, int tbpFlags);
        void RegisterTab(IntPtr hwndTab, IntPtr hwndMDI);
        void UnregisterTab(IntPtr hwndTab);
        void SetTabOrder(IntPtr hwndTab, IntPtr hwndInsertBefore);
        void SetTabActive(IntPtr hwndTab, IntPtr hwndMDI, int tbatFlags);
        void ThumbBarAddButtons(IntPtr hwnd, uint cButtons, [MarshalAs(UnmanagedType.LPArray)] ThumbButton[] pButtons);
        void ThumbBarUpdateButtons(IntPtr hwnd, uint cButtons, [MarshalAs(UnmanagedType.LPArray)] ThumbButton[] pButtons);
        void ThumbBarSetImageList(IntPtr hwnd, IntPtr himl);
        void SetOverlayIcon(IntPtr hwnd, IntPtr hIcon, [MarshalAs(UnmanagedType.LPWStr)] string pszDescription);
        void SetThumbnailTooltip(IntPtr hwnd, [MarshalAs(UnmanagedType.LPWStr)] string pszTip);
        void SetThumbnailClip(IntPtr hwnd, ref Rect prcClip);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct ThumbButton
    {
        public ThumbButtonMask dwMask;
        public uint iId;
        public uint iBitmap;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string szTip;
        public ThumbButtonFlags dwFlags;
    }

    [Flags]
    private enum ThumbButtonMask : uint
    {
        Bitmap = 0x0001,
        Icon = 0x0002,
        Tooltip = 0x0004,
        THB_FLAGS = 0x0008
    }

    [Flags]
    private enum ThumbButtonFlags : uint
    {
        Enabled = 0x0000,
        Disabled = 0x0001,
        DismissOnClick = 0x0002,
        NoBackground = 0x0004,
        Hidden = 0x0008,
        NonInteractive = 0x0010
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    private delegate IntPtr SubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, IntPtr dwRefData);

    [DllImport("comctl32.dll", SetLastError = true)]
    private static extern bool SetWindowSubclass(IntPtr hWnd, SubclassProc pfnSubclass, uint uIdSubclass, IntPtr dwRefData);

    [DllImport("comctl32.dll", SetLastError = true)]
    private static extern bool RemoveWindowSubclass(IntPtr hWnd, SubclassProc pfnSubclass, uint uIdSubclass);

    [DllImport("comctl32.dll", SetLastError = true)]
    private static extern IntPtr DefSubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    private static readonly Guid CLSID_TaskbarList = new("56fdf344-fd6d-11d0-958a-006097c9a090");

    public TaskbarThumbnailButtonService()
    {
        _taskbarList = (ITaskbarList3)Activator.CreateInstance(Type.GetTypeFromCLSID(CLSID_TaskbarList)!)!;
        _subclassProc = WindowSubclassProc;
    }

    /// <summary>
    /// 为指定窗口初始化缩略图工具栏按钮。
    /// </summary>
    public void Initialize(IntPtr hwnd)
    {
        if (_hwnd != IntPtr.Zero || hwnd == IntPtr.Zero)
        {
            return;
        }

        _hwnd = hwnd;
        _taskbarList.HrInit();

        SetWindowSubclass(hwnd, _subclassProc, 1, IntPtr.Zero);

        AddButtons(enabled: false);
    }

    /// <summary>
    /// 更新播放状态，按钮图标会在播放/暂停之间切换。
    /// </summary>
    public void UpdatePlaybackState(bool isPlaying, bool hasTrack)
    {
        if (_hwnd == IntPtr.Zero)
        {
            return;
        }

        AddButtons(hasTrack, isPlaying);
    }

    private void AddButtons(bool enabled, bool isPlaying = false)
    {
        ClearIcons();

        var flags = enabled ? ThumbButtonFlags.Enabled : ThumbButtonFlags.Disabled;

        var playPauseIcon = isPlaying ? IconGenerator.CreatePauseIcon() : IconGenerator.CreatePlayIcon();
        _iconHandles.Add(playPauseIcon);

        var previousIcon = IconGenerator.CreatePreviousIcon();
        _iconHandles.Add(previousIcon);

        var nextIcon = IconGenerator.CreateNextIcon();
        _iconHandles.Add(nextIcon);

        var buttons = new[]
        {
            new ThumbButton
            {
                dwMask = ThumbButtonMask.Icon | ThumbButtonMask.Tooltip | ThumbButtonMask.THB_FLAGS,
                iId = ThumbnailButtonPrevious,
                hIcon = previousIcon,
                szTip = "上一首",
                dwFlags = flags
            },
            new ThumbButton
            {
                dwMask = ThumbButtonMask.Icon | ThumbButtonMask.Tooltip | ThumbButtonMask.THB_FLAGS,
                iId = ThumbnailButtonPlayPause,
                hIcon = playPauseIcon,
                szTip = isPlaying ? "暂停" : "播放",
                dwFlags = flags
            },
            new ThumbButton
            {
                dwMask = ThumbButtonMask.Icon | ThumbButtonMask.Tooltip | ThumbButtonMask.THB_FLAGS,
                iId = ThumbnailButtonNext,
                hIcon = nextIcon,
                szTip = "下一首",
                dwFlags = flags
            }
        };

        if (_iconHandles.Count <= buttons.Length)
        {
            _taskbarList.ThumbBarAddButtons(_hwnd, (uint)buttons.Length, buttons);
        }
        else
        {
            _taskbarList.ThumbBarUpdateButtons(_hwnd, (uint)buttons.Length, buttons);
        }
    }

    private void ClearIcons()
    {
        foreach (var hIcon in _iconHandles)
        {
            if (hIcon != IntPtr.Zero)
            {
                DestroyIcon(hIcon);
            }
        }
        _iconHandles.Clear();
    }

    private IntPtr WindowSubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, IntPtr dwRefData)
    {
        if (uMsg == WmCommand)
        {
            var buttonId = (uint)((wParam.ToInt64() >> 16) & 0xFFFF);
            if (buttonId == 0)
            {
                buttonId = (uint)(wParam.ToInt64() & 0xFFFF);
            }

            switch (buttonId)
            {
                case ThumbnailButtonPrevious:
                    DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
                    {
                        App.PlaybackService.Previous();
                        SetForegroundWindow(hWnd);
                    });
                    return IntPtr.Zero;
                case ThumbnailButtonPlayPause:
                    DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
                    {
                        App.PlaybackService.PlayPause();
                        SetForegroundWindow(hWnd);
                    });
                    return IntPtr.Zero;
                case ThumbnailButtonNext:
                    DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
                    {
                        App.PlaybackService.Next();
                        SetForegroundWindow(hWnd);
                    });
                    return IntPtr.Zero;
            }
        }

        return DefSubclassProc(hWnd, uMsg, wParam, lParam);
    }

    public void Dispose()
    {
        if (_hwnd != IntPtr.Zero)
        {
            RemoveWindowSubclass(_hwnd, _subclassProc, 1);
            _hwnd = IntPtr.Zero;
        }

        ClearIcons();

        if (_taskbarList is not null)
        {
            Marshal.ReleaseComObject(_taskbarList);
        }
    }

    /// <summary>
    /// 图标生成器：使用 GDI+ 绘制简单的播放控制图标。
    /// </summary>
    private static class IconGenerator
    {
        private const int IconSize = 32;

        public static IntPtr CreatePlayIcon()
        {
            using var bitmap = new Bitmap(IconSize, IconSize);
            using var g = Graphics.FromImage(bitmap);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);

            var brush = Brushes.White;
            var points = new[]
            {
                new PointF(8, 6),
                new PointF(26, 16),
                new PointF(8, 26)
            };
            g.FillPolygon(brush, points);

            return bitmap.GetHicon();
        }

        public static IntPtr CreatePauseIcon()
        {
            using var bitmap = new Bitmap(IconSize, IconSize);
            using var g = Graphics.FromImage(bitmap);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);

            using var brush = new SolidBrush(Color.White);
            g.FillRectangle(brush, 7, 6, 7, 20);
            g.FillRectangle(brush, 18, 6, 7, 20);

            return bitmap.GetHicon();
        }

        public static IntPtr CreateNextIcon()
        {
            using var bitmap = new Bitmap(IconSize, IconSize);
            using var g = Graphics.FromImage(bitmap);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);

            var brush = Brushes.White;
            var points = new[]
            {
                new PointF(6, 6),
                new PointF(22, 16),
                new PointF(6, 26)
            };
            g.FillPolygon(brush, points);
            g.FillRectangle(brush, 22, 6, 4, 20);

            return bitmap.GetHicon();
        }

        public static IntPtr CreatePreviousIcon()
        {
            using var bitmap = new Bitmap(IconSize, IconSize);
            using var g = Graphics.FromImage(bitmap);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);

            var brush = Brushes.White;
            var points = new[]
            {
                new PointF(26, 6),
                new PointF(10, 16),
                new PointF(26, 26)
            };
            g.FillPolygon(brush, points);
            g.FillRectangle(brush, 6, 6, 4, 20);

            return bitmap.GetHicon();
        }
    }
}
