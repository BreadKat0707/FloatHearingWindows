using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.UI.Dispatching;
using Microsoft.Windows.AppLifecycle;

namespace FloatHearing.Services;

/// <summary>
/// 应用单实例管理：确保仅运行一个实例，并将后续启动激活转发给已有实例。
/// </summary>
public static class SingleInstanceService
{
    private const string InstanceKey = "FloatHearing";
    private const int SwRestore = 9;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    /// <summary>
    /// 注册单实例。若已有实例在运行，则转发激活并结束当前进程，返回 false。
    /// </summary>
    public static async Task<bool> TryRegisterAsync()
    {
        var instance = AppInstance.FindOrRegisterForKey(InstanceKey);

        if (!instance.IsCurrent)
        {
            var eventArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
            await instance.RedirectActivationToAsync(eventArgs).AsTask();
            ExitAsDuplicate();
            return false;
        }

        instance.Activated += OnAppActivated;
        return true;
    }

    /// <summary>
    /// 激活已有实例的主窗口。
    /// </summary>
    public static void ActivateMainWindow()
    {
        var window = App.MainWindow;
        if (window is null)
        {
            return;
        }

        var dispatcher = window.DispatcherQueue;
        if (dispatcher is null)
        {
            return;
        }

        dispatcher.TryEnqueue(() =>
        {
            try
            {
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                ShowWindow(hwnd, SwRestore);
                SetForegroundWindow(hwnd);
            }
            catch
            {
                // 忽略激活失败，回退到 Activate
                window.Activate();
            }
        });
    }

    private static void OnAppActivated(object? sender, AppActivationArguments e)
    {
        ActivateMainWindow();
    }

    private static void ExitAsDuplicate()
    {
        try
        {
            Process.GetCurrentProcess().Kill();
        }
        catch
        {
            Environment.Exit(0);
        }
    }
}
