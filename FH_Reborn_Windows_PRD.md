# FH Reborn — Windows 产品需求文档（PRD）

> 版本：v1.1 | 日期：2026-06-07
> 状态：需求收集中（持续补充）
> **App 名称：FH Reborn**（Float Hearing Reborn）
> 平台：Windows（桌面端）
> 其他平台文档：见 Android PRD / Linux PRD
> **说明**：本文档引用 Android PRD 中的通用功能定义，仅记录 Windows 平台特有的技术方案和待确认事项。

---

## 1. 项目概述

### 1.1 产品定位

一款面向本地音乐收藏爱好者的**高品质本地音乐播放器**，覆盖 Android（移动主力）、Linux（桌面）、Windows（桌面）三平台。各平台采用独立原生技术栈开发，追求极致的平台原生体验与设计一致性。

### 1.2 核心设计理念

- **视觉优先**：流体动态背景、Fluent Design 材质（云母/亚克力）、文字与图标的混合光效
- **播放体验**：类 Salt Player / Apple Music 的沉浸式播放界面，逐字歌词、弹簧动画
- **专业功能**：多格式歌词支持、播放统计、灵感记录、音频属性查看
- **跨平台一致**：统一的功能清单与设计规范，各平台按自身技术栈独立实现

### 1.3 开发策略

| 阶段 | 目标 | 内容 |
|------|------|------|
| **第一阶段（MVP）** | 可播放的媒体库 | 媒体库扫描 + 基础播放 + 基础 UI + LRC 歌词 |
| **第二阶段（增强）** | 体验提升 | FFmpeg 补充扫描 + TTML + 流体背景 + 律动 + 歌单系统 |
| **第三阶段（完善）** | 功能完备 | 数据持久化 + 导出 + 灵感记录 + 主题/模糊材质系统 |

### 1.4 Windows 技术栈

| 层级 | 技术选型 | 说明 |
|------|---------|------|
| **语言** | C# | .NET 生态系统，WinUI 3 原生支持 |
| **UI 框架** | WinUI 3 | Windows 官方原生 UI 框架 |
| **设计风格** | 原生 Fluent | 系统级 Fluent Design，原生 Mica/Acrylic/Reveal |
| **自绘引擎** | DirectX | WinUI 3 底层渲染 |
| **音频引擎** | MediaPlayer / NAudio | Windows 原生音频或 NAudio 库 |
| **数据库** | SQLite + EF Core | Entity Framework Core + SQLite |
| **元数据解析** | TagLib# | .NET 音频元数据解析 |
| **文件扫描** | System.IO + FFmpeg | .NET 文件 API + FFmpeg 补充 |
| **模糊效果** | 原生 Acrylic/Mica | WinUI 3 原生支持 |
| **IDE** | Visual Studio | |
| **构建工具** | MSBuild / .NET | 开发阶段建议 Self-Contained 部署 |

> **注意**：WinUI 3 存在 Runtime 版本地狱问题，开发阶段使用 Self-Contained 模式打包。

---

## 2. 用户画像

参见 Android PRD §2 用户画像。

---

## 3. 功能需求总览

### 3.1 功能模块图

功能模块与 Android 保持一致，参见 Android PRD §3.1。以下为 Windows 平台差异点：

| 模块 | Windows 差异 |
|------|-------------|
| 媒体库扫描 | `System.IO` 遍历用户指定目录，无 MediaStore |
| 后台播放 | 最小化到系统托盘，后台任务支持 |
| 媒体通知 | 系统媒体传输控制（SMTC）集成 |
| 音频焦点 | Windows 无系统级音频焦点，应用层自行处理 |
| 桌面歌词 | 桌面端不需要悬浮窗歌词（窗口化应用） |
| 状态栏歌词 | 不需要 |
| 手势控制 | 鼠标/触摸板为主，保留键盘快捷键 |
| 多尺寸适配 | 窗口缩放适配 |
| Reveal 光效 | WinUI 3 原生支持 Reveal Highlight |
| 云母/亚克力 | WinUI 3 原生支持 Mica/Acrylic 材质 |

---

## 4. 功能需求详述（待细化）

### 4.1 媒体库模块（F-001 ~ F-006）

**通用需求**：参见 Android PRD §4.1

**Windows 特有技术方案（待细化）**：

| 项 | 待确认 |
|----|--------|
| 目录选择 | `FolderPicker`（WinUI 3 原生） |
| 文件监控 | `FileSystemWatcher` (.NET) |
| 扫描性能 | `Parallel.ForEach` 并行遍历 |
| 元数据解析 | TagLib# 解析主流格式，FFmpeg 解析 APE/DSD |

### 4.2 播放器模块（F-007 ~ F-014）

**通用需求**：参见 Android PRD §4.2

**Windows 特有技术方案（待细化）**：

| 项 | 技术方案 |
|----|---------|
| 音频播放 | `MediaPlayer` (WinUI 内置) 或 NAudio |
| PCM 数据获取 | NAudio `WasapiCapture` / `SampleProvider` |
| 系统托盘 | WinUI 3 `TrayIcon` 扩展 |
| SMTC | `SystemMediaTransportControls` (WinUI 原生) |
| 键盘快捷键 | `KeyboardAccelerator` (WinUI 原生) + 全局热键注册 |
| 音频焦点 | 无系统级音频焦点，监听 SMTC Pause 事件 |

### 4.3 UI 与交互（F-015 ~ F-025）

**通用需求**：参见 Android PRD §4.3

**Windows 特有（待细化）**：

| 项 | 技术方案 |
|----|---------|
| Fluent Design | WinUI 3 原生 Fluent 主题 |
| 云母/亚克力 | `MicaBackdrop` / `DesktopAcrylicBackdrop` (WinUI 3 原生) |
| Reveal 光效 | WinUI 3 原生 `RevealBrush` / `RevealBorderBrush` |
| 流体背景 | WinUI 3 自定义 `CompositionBrush` + Win2D |
| 窗口管理 | 窗口缩放、最小化到托盘、标题栏自定义 |

### 4.4 歌词系统（F-026 ~ F-034）

**通用需求**：参见 Android PRD §4.4

**Windows 差异**：无桌面歌词（L-010）、无状态栏歌词（L-011），其他一致。

### 4.5 ~ 4.11 其他模块

通用需求参见 Android PRD 对应章节。

---

## 5. 设计风格规范

### 5.1 设计体系

| 平台 | 设计框架 | 材质效果 | 交互反馈 |
|------|---------|---------|---------|
| **Windows** | WinUI 3 原生 Fluent | 原生 Mica/Acrylic | **原生 Reveal** |

### 5.2 视觉要素

参见 Android PRD §5.2。Windows 上 Mica/Acrylic/Reveal 均为 WinUI 3 原生支持，无需自行实现。

---

## 6. MoSCoW 优先级矩阵

优先级与 Android 保持一致，参见 Android PRD §6。
**桌面歌词（L-010）和状态栏歌词（L-011）在 Windows 上为 Won\'t Have。**

---

## 7. 版本规划

与 Android 保持一致，参见 Android PRD §7。

---

## 8. 验收标准

参见 Android PRD §8。

---

## 9. 风险与约束

### 9.1 技术风险

| 风险 | 影响 | 概率 | 缓解措施 |
|------|------|------|----------|
| WinUI 3 Runtime 版本地狱 | 用户需安装对应 Windows App Runtime | 高 | **开发阶段使用 Self-Contained 打包** |
| WinUI 3 生态限制 | 第三方库相对较少 | 中 | 优先使用 WinUI 3 原生能力 |
| NAudio 可视化 | PCM 数据获取方案待验证 | 中 | 先用 NAudio `SampleProvider` 原型验证 |
| FFmpeg 集成 | Windows 下静态链接复杂 | 中 | 动态链接或 bundle FFmpeg 可执行文件 |

### 9.2 约束条件

- **最低 Windows 版本**：Windows 10 1809+（WinUI 3 最低要求），建议 Windows 11 以获得完整 Mica/Reveal 效果
- **部署方式**：Self-Contained（开发阶段）/ Framework-Dependent（正式发布后评估）
- **打包格式**：MSIX（推荐）/ 便携版 EXE

---

## 10. 开放问题

- [ ] **NAudio vs MediaPlayer** 音频播放方案最终确认
- [ ] **PCM 数据获取**方案需原型验证（NAudio `SampleProvider`）
- [ ] **WinUI 3 流体背景**具体实现（Win2D `CompositionBrush`）
- [ ] **系统托盘 + SMTC**集成方案
- [ ] **全局键盘快捷键**方案
- [ ] **打包方式**：MSIX / 便携 EXE / Self-Contained？
- [ ] **Windows 10/11 兼容性**测试
- [ ] 各功能模块的详细技术方案（参照 Android PRD 逐项细化）

---

## 11. 附录

### 11.1 相关文档索引

| 文档 | 说明 | 状态 |
|------|------|------|
| Android PRD | Android 详细需求 | ✅ 已完成 |
| Linux PRD | Linux 详细需求 | ✅ 框架完成，待细化 |
| **Windows PRD** | **本文档** | ✅ 框架完成，待细化 |

### 11.2 参考项目

- **WinUI 3**：https://learn.microsoft.com/windows/apps/winui/winui3/
- **TagLib#**：https://github.com/taglib/taglib
- **NAudio**：https://github.com/naudio/NAudio
- **Win2D**：https://github.com/microsoft/Win2D
- **AccordLegacy**（流体取色参考）：https://github.com/FoedusProgramme/AccordLegacy

### 11.3 术语表

参见 Android PRD §11.3。

---

> 本文档为 Windows 平台专用。通用功能定义参见 Android PRD。
