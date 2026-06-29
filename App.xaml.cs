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
    public static AppDbContext DbContext { get; } = new();

    /// <summary>
    /// 应用级播放服务。
    /// </summary>
    public static PlaybackService PlaybackService { get; } = new();

    /// <summary>
    /// Initializes the singleton application object.
    /// </summary>
    public App()
    {
        InitializeComponent();

        var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
        DbContext.DbPath = Path.Combine(localFolder, "floathearing_v2.db");
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        await DbContext.Database.EnsureCreatedAsync();

        MainWindow = new MainWindow();
        MainWindow.Activate();
    }
}
