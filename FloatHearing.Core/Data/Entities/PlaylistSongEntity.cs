namespace FloatHearing.Data.Entities;

/// <summary>
/// 歌单与歌曲的多对多关联实体
/// </summary>
public sealed class PlaylistSongEntity
{
    public long PlaylistId { get; set; }

    public PlaylistEntity Playlist { get; set; } = null!;

    public long SongId { get; set; }

    public SongEntity Song { get; set; } = null!;

    public int SortOrder { get; set; }

    public DateTime AddedAt { get; set; }
}
