namespace FloatHearing.Models;

/// <summary>
/// 专辑 UI 模型
/// </summary>
public sealed class Album
{
    public string Title { get; set; } = string.Empty;

    public string Artist { get; set; } = string.Empty;

    public int SongCount { get; set; }

    public TimeSpan TotalDuration { get; set; }

    public string? CoverPath { get; set; }

    public string? ReleaseDateText { get; set; }

    public override string ToString() => string.IsNullOrWhiteSpace(Title) ? "未知专辑" : Title;
}
