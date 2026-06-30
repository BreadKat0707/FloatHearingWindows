using Microsoft.UI.Xaml;
using FloatHearing.Data;
using FloatHearing.Services;

namespace FloatHearing;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// 主窗口引用，供文件/文件夹选择器等需要 HWND 的 API 使用。
    /// </summary>
    public static Window? MainWindow { get; private set; }

    /// <summary>
    /// 应用数据库上下文。
    /// </summary>
    public static AppDbContext DbContext { get; private set; } = null!;

    /// <summary>
    /// 应用级播放服务。
    /// </summary>
    public static PlaybackService PlaybackService { get; private set; } = null!;

    /// <summary>
    /// 应用设置服务。
    /// </summary>
    public static SettingsService SettingsService { get; private set; } = null!;

    /// <summary>
    /// Initializes the singleton application object.
    /// </summary>
    public App()
    {
        InitializeComponent();

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

        var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
        DbContext = new AppDbContext
        {
            DbPath = Path.Combine(localFolder, "floathearing_v2.db")
        };
        PlaybackService = new PlaybackService(DbContext);
        SettingsService = new SettingsService(DbContext);
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        if (!await SingleInstanceService.TryRegisterAsync())
        {
            return;
        }

        await InitializeDatabaseAsync();
        await PlaybackService.InitializeAsync();
        await SettingsService.LoadAsync();

        MainWindow = new MainWindow();
        MainWindow.Activate();

        InitializeTaskbarThumbnailButtons();

        await ShowCrashReportIfNeededAsync();
    }

    private static void InitializeTaskbarThumbnailButtons()
    {
        try
        {
            if (MainWindow is null)
            {
                return;
            }

            var service = new TaskbarThumbnailButtonService();

            MainWindow.Activated += (sender, args) =>
            {
                if (args.WindowActivationState != WindowActivationState.Deactivated)
                {
                    return;
                }

                try
                {
                    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow);
                    service.Initialize(hwnd);
                    service.UpdatePlaybackState(
                        App.PlaybackService.IsPlaying,
                        App.PlaybackService.CurrentSong is not null);
                }
                catch
                {
                    // 忽略初始化失败
                }
            };

            // 如果窗口已经激活，直接初始化
            if (MainWindow.Visible)
            {
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow);
                service.Initialize(hwnd);
                service.UpdatePlaybackState(
                    App.PlaybackService.IsPlaying,
                    App.PlaybackService.CurrentSong is not null);
            }

            App.PlaybackService.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(PlaybackService.IsPlaying) || e.PropertyName == nameof(PlaybackService.CurrentSong))
                {
                    service.UpdatePlaybackState(
                        App.PlaybackService.IsPlaying,
                        App.PlaybackService.CurrentSong is not null);
                }
            };
        }
        catch (Exception ex)
        {
            CrashReportService.SaveCrashInfo(ex);
        }
    }

    private static async Task ShowCrashReportIfNeededAsync()
    {
        if (!CrashReportService.HasPendingReport())
        {
            return;
        }

        var report = CrashReportService.ReadAndClearReport();
        if (string.IsNullOrWhiteSpace(report))
        {
            return;
        }

        var textBox = new Microsoft.UI.Xaml.Controls.TextBox
        {
            Text = report,
            IsReadOnly = true,
            AcceptsReturn = true,
            TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
            Height = 300,
            FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Consolas")
        };

        var copyButton = new Microsoft.UI.Xaml.Controls.Button
        {
            Content = "复制到剪贴板"
        };

        var panel = new Microsoft.UI.Xaml.Controls.StackPanel
        {
            Spacing = 12,
            Children = { textBox, copyButton }
        };

        var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
        {
            Title = "上次启动发生崩溃",
            Content = panel,
            CloseButtonText = "关闭",
            XamlRoot = MainWindow?.Content?.XamlRoot
        };

        copyButton.Click += (s, e) =>
        {
            CrashReportService.CopyToClipboard(report);
            copyButton.Content = "已复制";
        };

        try
        {
            await dialog.ShowAsync();
        }
        catch
        {
            // 忽略弹窗失败
        }
    }

    private static void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            CrashReportService.SaveCrashInfo(ex);
        }
    }

    private static void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        CrashReportService.SaveCrashInfo(e.Exception);
        e.SetObserved();
    }

    private static async Task InitializeDatabaseAsync()
    {
        try
        {
            // 修复早期版本遗留的空数据库文件：若文件大小为 0，删除后重新创建。
            var dbPath = DbContext.DbPath;
            var fileInfo = new FileInfo(dbPath);
            if (fileInfo.Exists && fileInfo.Length == 0)
            {
                fileInfo.Delete();
            }

            await DbContext.Database.EnsureCreatedAsync();
        }
        catch
        {
            // 若自动初始化失败，仍让应用启动，避免崩溃。
        }
    }
}
