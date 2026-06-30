namespace FloatHearing.Data.Entities;

/// <summary>
/// 歌单数据库实体
/// </summary>
public sealed class PlaylistEntity
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? CoverPath { get; set; }

    /// <summary>
    /// 默认播放模式：0=顺序，1=列表循环，2=单曲循环，3=随机
    /// </summary>
    public int DefaultPlayMode { get; set; }

    public string? Tags { get; set; }

    public int SortType { get; set; }

    public bool IsSystem { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<PlaylistSongEntity> PlaylistSongs { get; set; } = [];
}
