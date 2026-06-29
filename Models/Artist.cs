namespace FloatHearing.Models;

/// <summary>
/// 艺术家 UI 模型
/// </summary>
public sealed class Artist
{
    public string Name { get; set; } = string.Empty;

    public int SongCount { get; set; }

    public int AlbumCount { get; set; }

    public TimeSpan TotalDuration { get; set; }

    public override string ToString() => string.IsNullOrWhiteSpace(Name) ? "未知艺术家" : Name;
}
