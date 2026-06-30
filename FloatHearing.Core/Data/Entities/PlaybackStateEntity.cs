namespace FloatHearing.Data.Entities;

/// <summary>
/// 播放状态单例实体（Id 固定为 1）
/// </summary>
public sealed class PlaybackStateEntity
{
    public int Id { get; set; }

    public long? CurrentSongId { get; set; }

    public long PositionMs { get; set; }

    public string QueueJson { get; set; } = "[]";

    /// <summary>
    /// 循环模式：0=顺序，1=列表循环，2=单曲循环
    /// </summary>
    public int RepeatMode { get; set; }

    public bool ShuffleMode { get; set; }

    public double Volume { get; set; } = 0.8;

    public DateTime UpdatedAt { get; set; }
}
