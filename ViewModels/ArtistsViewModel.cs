using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using FloatHearing.Data;
using FloatHearing.Models;

namespace FloatHearing.ViewModels;

/// <summary>
/// 艺术家页视图模型
/// </summary>
public sealed class ArtistsViewModel : INotifyPropertyChanged
{
    private readonly AppDbContext _dbContext;

    public ObservableCollection<Artist> Artists { get; } = [];

    public ArtistsViewModel(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task LoadArtistsAsync(CancellationToken cancellationToken = default)
    {
        Artists.Clear();

        var songs = await _dbContext.Songs
            .AsNoTracking()
            .Where(s => !string.IsNullOrEmpty(s.ArtistNames))
            .Select(s => new { s.ArtistNames, s.AlbumArtistNames, s.DurationMs })
            .ToListAsync(cancellationToken);

        var artistStats = new Dictionary<string, (int SongCount, int AlbumCount, long DurationMs)>(StringComparer.OrdinalIgnoreCase);

        foreach (var song in songs)
        {
            var names = song.ArtistNames
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(n => n.Trim())
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (var name in names)
            {
                if (!artistStats.TryGetValue(name, out var stats))
                {
                    stats = (0, 0, 0);
                }

                artistStats[name] = (stats.SongCount + 1, stats.AlbumCount, stats.DurationMs + song.DurationMs);
            }
        }

        // 统计每个艺术家的专辑数（基于 AlbumArtistNames）
        var albumArtists = await _dbContext.Songs
            .AsNoTracking()
            .Where(s => !string.IsNullOrEmpty(s.AlbumArtistNames) && !string.IsNullOrEmpty(s.Album))
            .Select(s => new { s.AlbumArtistNames, s.Album })
            .ToListAsync(cancellationToken);

        var artistAlbums = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in albumArtists)
        {
            var names = item.AlbumArtistNames
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(n => n.Trim())
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (var name in names)
            {
                if (!artistAlbums.TryGetValue(name, out var albums))
                {
                    albums = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    artistAlbums[name] = albums;
                }

                albums.Add(item.Album);
            }
        }

        var artists = artistStats
            .Select(kvp => new Artist
            {
                Name = kvp.Key,
                SongCount = kvp.Value.SongCount,
                AlbumCount = artistAlbums.TryGetValue(kvp.Key, out var albums) ? albums.Count : 0,
                TotalDuration = TimeSpan.FromMilliseconds(kvp.Value.DurationMs)
            })
            .OrderBy(a => a.Name)
            .ToList();

        foreach (var artist in artists)
        {
            Artists.Add(artist);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
