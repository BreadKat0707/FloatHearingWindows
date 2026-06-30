namespace FloatHearing.Data.Entities;

/// <summary>
/// 歌曲数据库实体
/// </summary>
public sealed class SongEntity
{
    public long Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Artist { get; set; } = string.Empty;

    public string Album { get; set; } = string.Empty;

    public string AlbumArtist { get; set; } = string.Empty;

    /// <summary>
    /// 音频文件完整路径，唯一索引。
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    public long DurationMs { get; set; }

    public int? DiscNumber { get; set; }

    public int? TrackNumber { get; set; }

    public int? ReleaseYear { get; set; }

    public DateTime? ReleaseDate { get; set; }

    public int? Bitrate { get; set; }

    public int? SampleRate { get; set; }

    public int? Channels { get; set; }

    public long FileSize { get; set; }

    public string FileName { get; set; } = string.Empty;

    public long FileModifiedAt { get; set; }

    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// 封面缓存相对路径（相对于应用本地文件夹）。
    /// </summary>
    public string? CoverPath { get; set; }

    public bool IsFavorite { get; set; }

    public int Rating { get; set; }

    public DateTime DateAdded { get; set; }

    public DateTime DateModified { get; set; }

    public ICollection<PlaylistSongEntity> PlaylistSongs { get; set; } = [];

    public ICollection<InspirationNoteEntity> InspirationNotes { get; set; } = [];

    public PlaybackStatsEntity? PlaybackStats { get; set; }
}
