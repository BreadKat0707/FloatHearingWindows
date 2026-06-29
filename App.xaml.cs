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
    /// Initializes the singleton application object.
    /// </summary>
    public App()
    {
        InitializeComponent();

        var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
        DbContext = new AppDbContext
        {
            DbPath = Path.Combine(localFolder, "floathearing_v2.db")
        };
        PlaybackService = new PlaybackService(DbContext);
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        await InitializeDatabaseAsync();
        await PlaybackService.InitializeAsync();

        MainWindow = new MainWindow();
        MainWindow.Activate();
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
