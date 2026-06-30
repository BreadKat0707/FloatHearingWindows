namespace FloatHearing.Data.Entities;

/// <summary>
/// 音乐灵感记录实体
/// </summary>
public sealed class InspirationNoteEntity
{
    public long Id { get; set; }

    public long SongId { get; set; }

    public SongEntity Song { get; set; } = null!;

    public long PositionMs { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
