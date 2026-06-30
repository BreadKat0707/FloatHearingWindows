using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using FloatHearing.Data;
using FloatHearing.Data.Entities;
using SkiaSharp;

namespace FloatHearing.Services;

/// <summary>
/// 扫描进度信息
/// </summary>
public sealed class ScanProgress
{
    public string Phase { get; init; } = string.Empty;

    public int ProcessedCount { get; init; }

    public int TotalCount { get; init; }

    public string CurrentPath { get; init; } = string.Empty;
}

/// <summary>
/// 本地音乐库扫描服务
/// </summary>
public sealed class LibraryScanner
{
    private readonly AppDbContext _dbContext;
    private readonly string _coversDirectory;

    private static readonly string[] AudioExtensions =
    [
        ".mp3", ".flac", ".wav", ".m4a", ".wma",
        ".aac", ".ogg", ".opus", ".ape", ".dsf", ".dff"
    ];

    public LibraryScanner(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        _coversDirectory = Path.Combine(Path.GetDirectoryName(dbContext.DbPath) ?? Path.GetTempPath(), "Covers");
    }

    /// <summary>
    /// 扫描指定目录并将结果同步到数据库。
    /// </summary>
    public async Task ScanAsync(
        IEnumerable<string> paths,
        IProgress<ScanProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var scanPaths = paths.Where(Directory.Exists).ToList();
        if (scanPaths.Count == 0)
        {
            return;
        }

        Directory.CreateDirectory(_coversDirectory);

        progress?.Report(new ScanProgress { Phase = "正在发现文件..." });

        var hiddenFolders = await _dbContext.HiddenFolders
            .AsNoTracking()
            .Select(h => h.Path)
            .ToListAsync(cancellationToken);

        var skippedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in scanPaths)
        {
            DiscoverSkippedDirectories(path, hiddenFolders, skippedDirectories);
        }

        var audioFiles = new List<string>();
        foreach (var path in scanPaths)
        {
            EnumerateAudioFiles(path, skippedDirectories, audioFiles);
        }

        progress?.Report(new ScanProgress { Phase = $"发现 {audioFiles.Count} 个音频文件", TotalCount = audioFiles.Count });

        var existingSongs = await _dbContext.Songs
            .AsNoTracking()
            .ToDictionaryAsync(s => s.FilePath, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var processedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var updatedSongs = new List<SongEntity>();

        for (int i = 0; i < audioFiles.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var filePath = audioFiles[i];
            processedPaths.Add(filePath);

            progress?.Report(new ScanProgress
            {
                Phase = "正在读取元数据...",
                ProcessedCount = i + 1,
                TotalCount = audioFiles.Count,
                CurrentPath = filePath
            });

            var fileInfo = new FileInfo(filePath);
            var modifiedAt = fileInfo.LastWriteTimeUtc.Ticks;

            if (existingSongs.TryGetValue(filePath, out var existing)
                && existing.FileModifiedAt == modifiedAt)
            {
                continue;
            }

            var song = await ReadSongMetadataAsync(filePath, fileInfo, existing);
            if (song is null)
            {
                continue;
            }

            if (existing is null)
            {
                _dbContext.Songs.Add(song);
            }
            else
            {
                song.Id = existing.Id;
                song.DateAdded = existing.DateAdded;
                _dbContext.Songs.Update(song);
            }

            updatedSongs.Add(song);

            // 定期保存，避免单次事务过大
            if (updatedSongs.Count >= 100)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                updatedSongs.Clear();
            }
        }

        if (updatedSongs.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        progress?.Report(new ScanProgress { Phase = "正在清理已移除的文件...", TotalCount = audioFiles.Count });

        var missingPaths = existingSongs.Keys.Except(processedPaths, StringComparer.OrdinalIgnoreCase).ToList();
        if (missingPaths.Count > 0)
        {
            var missingSongs = await _dbContext.Songs
                .Where(s => missingPaths.Contains(s.FilePath))
                .ToListAsync(cancellationToken);

            _dbContext.Songs.RemoveRange(missingSongs);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        progress?.Report(new ScanProgress { Phase = "扫描完成", TotalCount = audioFiles.Count });
    }

    private void DiscoverSkippedDirectories(string rootPath, List<string> hiddenFolders, HashSet<string> skipped)
    {
        try
        {
            foreach (var directory in Directory.EnumerateDirectories(rootPath, "*", SearchOption.AllDirectories))
            {
                if (hiddenFolders.Any(h => directory.StartsWith(h, StringComparison.OrdinalIgnoreCase)))
                {
                    skipped.Add(directory);
                    continue;
                }

                if (File.Exists(Path.Combine(directory, ".nomedia")))
                {
                    skipped.Add(directory);
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            // 忽略无权限目录
        }
    }

    private void EnumerateAudioFiles(string rootPath, HashSet<string> skippedDirectories, List<string> results)
    {
        try
        {
            var options = new EnumerationOptions
            {
                RecurseSubdirectories = true,
                IgnoreInaccessible = true
            };

            foreach (var file in Directory.EnumerateFiles(rootPath, "*.*", options))
            {
                var extension = Path.GetExtension(file);
                if (!AudioExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                var directory = Path.GetDirectoryName(file);
                if (directory is not null && IsDirectorySkipped(directory, skippedDirectories))
                {
                    continue;
                }

                results.Add(file);
            }
        }
        catch (UnauthorizedAccessException)
        {
            // 忽略无权限目录
        }
    }

    private static bool IsDirectorySkipped(string directory, HashSet<string> skippedDirectories)
    {
        foreach (var skipped in skippedDirectories)
        {
            if (directory.StartsWith(skipped, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 重新读取单个文件的元数据。
    /// </summary>
    public async Task<SongEntity?> RescanSingleAsync(string filePath, FileInfo fileInfo, SongEntity? existing)
    {
        return await ReadSongMetadataAsync(filePath, fileInfo, existing);
    }

    private async Task<SongEntity?> ReadSongMetadataAsync(string filePath, FileInfo fileInfo, SongEntity? existing)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var tagFile = TagLib.File.Create(filePath);
                var tag = tagFile.Tag;
                var properties = tagFile.Properties;

                var title = string.IsNullOrWhiteSpace(tag.Title)
                    ? Path.GetFileNameWithoutExtension(filePath)
                    : tag.Title;
                var artist = string.IsNullOrWhiteSpace(tag.FirstPerformer) ? "未知艺术家" : tag.FirstPerformer;
                var album = string.IsNullOrWhiteSpace(tag.Album) ? "未知专辑" : tag.Album;
                var albumArtist = string.IsNullOrWhiteSpace(tag.FirstAlbumArtist) ? artist : tag.FirstAlbumArtist;

                var durationMs = (long)properties.Duration.TotalMilliseconds;
                var coverPath = existing?.CoverPath;

                var discNumber = tag.Disc > 0 ? (int?)tag.Disc : null;
                var trackNumber = tag.Track > 0 ? (int?)tag.Track : null;
                var releaseYear = tag.Year > 0 ? (int?)tag.Year : null;
                var releaseDate = releaseYear.HasValue ? new DateTime(releaseYear.Value, 1, 1) : (DateTime?)null;

                var pictures = tag.Pictures;
                if (pictures is { Length: > 0 })
                {
                    coverPath = SaveCover(filePath, pictures[0]);
                }

                return new SongEntity
                {
                    Title = title,
                    Artist = artist,
                    Album = album,
                    AlbumArtist = albumArtist,
                    FilePath = filePath,
                    FileName = Path.GetFileName(filePath),
                    DurationMs = durationMs,
                    DiscNumber = discNumber,
                    TrackNumber = trackNumber,
                    ReleaseYear = releaseYear,
                    ReleaseDate = releaseDate,
                    Bitrate = properties.AudioBitrate,
                    SampleRate = properties.AudioSampleRate,
                    Channels = properties.AudioChannels,
                    FileSize = fileInfo.Length,
                    FileModifiedAt = fileInfo.LastWriteTimeUtc.Ticks,
                    Format = properties.Description,
                    CoverPath = coverPath,
                    IsFavorite = existing?.IsFavorite ?? false,
                    Rating = existing?.Rating ?? 0,
                    DateAdded = existing?.DateAdded ?? DateTime.UtcNow,
                    DateModified = DateTime.UtcNow
                };
            }
            catch
            {
                return null;
            }
        });
    }

    private string? SaveCover(string filePath, TagLib.IPicture picture)
    {
        try
        {
            if (picture.Data.IsEmpty)
            {
                return null;
            }

            var hash = ComputeHash(filePath);
            var coverFileName = $"{hash}.jpg";
            var coverPath = Path.Combine(_coversDirectory, coverFileName);
            var relativePath = Path.Combine("Covers", coverFileName);

            // 使用 SkiaSharp 缩放为最大边 400 的缩略图，节省磁盘空间并提高列表性能
            using var inputBitmap = SKBitmap.Decode(picture.Data.Data);
            if (inputBitmap is null)
            {
                return null;
            }

            var (targetWidth, targetHeight) = CalculateScaledSize(inputBitmap.Width, inputBitmap.Height, 400);
            using var resizedBitmap = inputBitmap.Resize(new SKSizeI(targetWidth, targetHeight), new SKSamplingOptions(SKCubicResampler.Mitchell));
            if (resizedBitmap is null)
            {
                return null;
            }

            using var encodedData = resizedBitmap.Encode(SKEncodedImageFormat.Jpeg, 90);
            if (encodedData is null)
            {
                return null;
            }

            File.WriteAllBytes(coverPath, encodedData.ToArray());
            return relativePath;
        }
        catch
        {
            return null;
        }
    }

    private static (int Width, int Height) CalculateScaledSize(int originalWidth, int originalHeight, int maxDimension)
    {
        if (originalWidth <= maxDimension && originalHeight <= maxDimension)
        {
            return (originalWidth, originalHeight);
        }

        var ratio = Math.Min((double)maxDimension / originalWidth, (double)maxDimension / originalHeight);
        return ((int)(originalWidth * ratio), (int)(originalHeight * ratio));
    }

    private static string ComputeHash(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant()[..16];
    }
}
