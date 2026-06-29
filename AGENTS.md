# Float Hearing - Agent 说明

## 技术栈

- WinUI 3（Windows App SDK 2.2）
- .NET 10
- C# 12+
- 打包应用（Packaged，使用 MSIX）
- 数据层：`FloatHearing.Core` 类库（EF Core + SQLite + TagLib#）

## 常用命令

```powershell
# 构建整个解决方案
dotnet build FloatHearing.slnx

# 调试运行
dotnet run --project FloatHearing.csproj

# 发布 Release
dotnet publish FloatHearing.csproj -c Release
```

## 代码组织

- `FloatHearing.Core/`：数据层与服务层类库，不依赖 WinUI。
  - `Data/Entities/`：数据库实体。
  - `Data/AppDbContext.cs`：EF Core DbContext。
  - `Services/LibraryScanner.cs`：媒体库扫描服务。
- `FloatHearing/`：WinUI 3 应用层。
  - `Models/`：UI 模型（Song、Album、Artist）。
  - `ViewModels/`：实现 `INotifyPropertyChanged` 的视图模型。
  - `Pages/`：各功能页面（Songs、Albums、Artists、Playlists、Ideas、Stats、Settings）。
  - `Services/PlaybackService.cs`：应用级单例播放服务。
  - `Converters/`：XAML 值转换器。
  - `MainPage.xaml.cs`：NavigationView 导航外壳与底部播放栏。
  - `MainWindow.xaml.cs`：主窗口与 TitleBar。

## 编码约定

- 使用文件级命名空间（C# 10+）。
- 优先使用 `x:Bind` 进行 XAML 绑定。
- 对不变化的属性使用 `Mode=OneTime` 以避免 XAML 编译器警告。
- 新增页面/窗口可使用已安装的 WinUI 模板：`dotnet new winui-page` / `dotnet new winui-window`。

## 注意事项

- 数据层迁移：由于 WinUI 3 项目的模块初始化器与 `dotnet ef` 不兼容，当前使用 `EnsureCreatedAsync()` 进行数据库初始化。后续如需正式迁移，可将 `FloatHearing.Core` 拆分为独立的可执行项目来生成迁移。
- 文件/文件夹选择器需要 HWND 初始化，统一通过 `App.MainWindow` 获取句柄。
- 音频播放当前使用 `Windows.Media.Playback.MediaPlayer`，后续如需更复杂的音频处理可考虑 NAudio 或 WASAPI。
- 项目为打包应用，直接运行需要 Windows App Runtime；`dotnet run` 会通过 `Microsoft.Windows.SDK.BuildTools.WinApp` 自动处理调试身份。
- `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 存在上游安全漏洞警告，需等待 EF Core/SQLitePCLRaw 发布新版本修复。
