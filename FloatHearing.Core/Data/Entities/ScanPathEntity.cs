namespace FloatHearing.Data.Entities;

/// <summary>
/// 用户指定的媒体库扫描路径
/// </summary>
public sealed class ScanPathEntity
{
    public long Id { get; set; }

    public string Path { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
