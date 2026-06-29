namespace FloatHearing.Models;

/// <summary>
/// 歌曲 UI 模型
/// </summary>
public sealed class Song
{
    public long Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Artist { get; set; } = string.Empty;

    public string Album { get; set; } = string.Empty;

    public string AlbumArtist { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public long FileModifiedAt { get; set; }

    public TimeSpan Duration { get; set; }

    public int? DiscNumber { get; set; }

    public int? TrackNumber { get; set; }

    public int? ReleaseYear { get; set; }

    public DateTime? ReleaseDate { get; set; }

    public DateTime DateAdded { get; set; }

    public string? CoverPath { get; set; }

    public int PlayCount { get; set; }

    public bool IsFavorite { get; set; }

    public int Rating { get; set; }

    public override string ToString() => string.IsNullOrWhiteSpace(Title) ? System.IO.Path.GetFileName(FilePath) : Title;
}
