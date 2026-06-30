namespace FloatHearing.Data.Entities;

/// <summary>
/// 歌曲播放统计实体
/// </summary>
public sealed class PlaybackStatsEntity
{
    public long SongId { get; set; }

    public SongEntity Song { get; set; } = null!;

    public int PlayCount { get; set; }

    public long TotalPlayedMs { get; set; }

    public DateTime? LastPlayedAt { get; set; }

    public int SkipCount { get; set; }

    public DateTime UpdatedAt { get; set; }
}
