# Float Hearing

一款面向本地音乐收藏爱好者的 Windows 本地音乐播放器，基于 WinUI 3 与 Windows App SDK 构建（FH Reborn Windows 端）。

## 技术栈

- **UI 框架**：WinUI 3 / Windows App SDK 2.2
- **目标平台**：Windows 10 版本 19041 及以上 / Windows 11
- **.NET**：.NET 10
- **语言**：C# 12+
- **ORM**：Entity Framework Core + SQLite
- **元数据解析**：TagLib#

## 已具备的功能

- 扫描本地文件夹中的音频文件（支持 MP3、FLAC、WAV、M4A、WMA、AAC、OGG、OPUS、APE、DSD）
- 使用 TagLib# 读取完整音乐元数据（标题、艺术家、专辑、时长、比特率、采样率、声道）
- 内嵌专辑封面提取与缓存
- SQLite 持久化媒体库，支持增量更新
- 播放 / 暂停、上一首、下一首
- 播放进度与音量滑块
- 原生 Mica 背景与 TitleBar

## 项目结构

```
FloatHearing/
├── App.xaml / App.xaml.cs        # 应用入口
├── MainWindow.xaml / .cs         # 主窗口与标题栏
├── MainPage.xaml / .cs           # 导航外壳（侧边栏 + 内容 Frame + 播放控制栏）
├── Models/
│   ├── Song.cs                   # 歌曲 UI 模型
│   ├── Album.cs                  # 专辑 UI 模型
│   └── Artist.cs                 # 艺术家 UI 模型
├── ViewModels/
│   ├── ShellViewModel.cs         # 导航外壳视图模型
│   ├── SongsViewModel.cs         # 歌曲页视图模型
│   ├── AlbumsViewModel.cs        # 专辑页视图模型
│   └── ArtistsViewModel.cs       # 艺术家页视图模型
├── Pages/
│   ├── SongsPage.xaml            # 歌曲页
│   ├── AlbumsPage.xaml           # 专辑页
│   ├── ArtistsPage.xaml          # 艺术家页
│   ├── PlaylistsPage.xaml        # 歌单页
│   ├── IdeasPage.xaml            # 想法页
│   ├── StatsPage.xaml            # 统计页
│   └── SettingsPage.xaml         # 设置页
├── Services/
│   └── PlaybackService.cs        # 应用级播放服务
├── Converters/                   # XAML 值转换器
├── Assets/                       # 应用图标与启动图
├── Package.appxmanifest          # 应用清单
└── FloatHearing.csproj

FloatHearing.Core/                # 数据层与服务层类库
├── Data/
│   ├── Entities/                 # 数据库实体
│   ├── AppDbContext.cs           # EF Core DbContext
│   └── Migrations/               # 迁移目录（预留）
└── Services/
    └── LibraryScanner.cs         # 媒体库扫描服务
```

## 构建与运行

```powershell
# 还原并编译
dotnet build FloatHearing.slnx

# 运行调试版本
dotnet run --project FloatHearing.csproj

# 发布（Release）
dotnet publish FloatHearing.csproj -c Release
```

## 后续计划

- [ ] 播放队列与播放模式（顺序/循环/单曲/随机）
- [ ] SMTC 系统媒体传输控制
- [ ] 系统托盘最小化
- [ ] LRC 歌词解析与显示
- [ ] 歌单管理
- [ ] 搜索与排序
