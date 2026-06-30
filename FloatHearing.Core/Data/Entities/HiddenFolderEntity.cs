namespace FloatHearing.Data.Entities;

/// <summary>
/// 用户隐藏/排除的文件夹路径
/// </summary>
public sealed class HiddenFolderEntity
{
    public string Path { get; set; } = string.Empty;

    public DateTime AddedAt { get; set; }
}
