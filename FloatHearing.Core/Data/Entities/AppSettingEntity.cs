using Microsoft.EntityFrameworkCore;

namespace FloatHearing.Data.Entities;

/// <summary>
/// 主题颜色模式。
/// </summary>
public enum ThemeMode
{
    System,
    Light,
    Dark
}

/// <summary>
/// 窗口背景材质。
/// </summary>
public enum BackdropMaterial
{
    Solid,
    Acrylic,
    Mica,
    MicaAlt
}

/// <summary>
/// 应用设置实体。
/// </summary>
public sealed class AppSettingEntity
{
    public int Id { get; set; }

    /// <summary>
    /// 主题颜色模式。
    /// </summary>
    public ThemeMode ThemeMode { get; set; } = ThemeMode.System;

    /// <summary>
    /// 窗口背景材质。
    /// </summary>
    public BackdropMaterial BackdropMaterial { get; set; } = BackdropMaterial.Solid;

    /// <summary>
    /// 显示语言标签，例如 zh-CN、en-US。
    /// </summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// 歌曲列表排序字段。
    /// </summary>
    public int SongSortField { get; set; }

    /// <summary>
    /// 歌曲列表排序方向。
    /// </summary>
    public int SortDirection { get; set; }

    /// <summary>
    /// 艺术家分隔符，多个分隔符用空格分隔。
    /// </summary>
    public string ArtistSeparators { get; set; } = "& / , 、 feat. ft. FEAT. FT. Feat. Ft.";
}
