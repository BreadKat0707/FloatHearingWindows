namespace FloatHearing.Core.Models;

/// <summary>
/// 歌曲排序字段
/// </summary>
public enum SongSortField
{
    Title,
    AlbumDiscTrack,
    Duration,
    FileSize,
    FileModifiedAt,
    DateAdded,
    FileName,
    ArtistAlbum,
    ReleaseDate,
    PlayCount
}

/// <summary>
/// 排序方向
/// </summary>
public enum SortDirection
{
    Ascending,
    Descending
}

/// <summary>
/// 排序字段显示名称
/// </summary>
public static class SongSortFieldExtensions
{
    public static string GetDisplayName(this SongSortField field) => field switch
    {
        SongSortField.Title => "标题",
        SongSortField.AlbumDiscTrack => "专辑-碟号-音轨号",
        SongSortField.Duration => "曲目时长",
        SongSortField.FileSize => "文件大小",
        SongSortField.FileModifiedAt => "修改时间",
        SongSortField.DateAdded => "添加时间",
        SongSortField.FileName => "文件名",
        SongSortField.ArtistAlbum => "艺术家-专辑",
        SongSortField.ReleaseDate => "发行时间",
        SongSortField.PlayCount => "播放次数",
        _ => field.ToString()
    };
}
