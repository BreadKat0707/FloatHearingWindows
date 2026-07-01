using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using FloatHearing.Data;
using FloatHearing.Models;

namespace FloatHearing.ViewModels;

/// <summary>
/// 艺术家详情页视图模型
/// </summary>
public sealed class ArtistDetailViewModel : INotifyPropertyChanged
{
    private readonly AppDbContext _dbContext;

    public Artist Artist { get; }

    public ObservableCollection<Song> Songs { get; } = [];

    public ObservableCollection<Album> Albums { get; } = [];

    public ObservableCollection<Album> FeaturedAlbums { get; } = [];

    public ArtistDetailViewModel(AppDbContext dbContext, Artist artist)
    {
        _dbContext = dbContext;
        Artist = artist;
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await LoadSongsAsync(cancellationToken);
        await LoadAlbumsAsync(cancellationToken);
        await LoadFeaturedAlbumsAsync(cancellationToken);
    }

    private async Task LoadSongsAsync(CancellationToken cancellationToken)
    {
        Songs.Clear();

        var songs = await _dbContext.Songs
            .AsNoTracking()
            .Where(s => s.Artist == Artist.Name)
            .OrderBy(s => s.Album)
            .ThenBy(s => s.DiscNumber ?? 0)
            .ThenBy(s => s.TrackNumber ?? 0)
            .Select(s => new Song
            {
                Id = s.Id,
                Title = s.Title,
                Artist = s.Artist,
                Album = s.Album,
                AlbumArtist = s.AlbumArtist,
                FilePath = s.FilePath,
                FileName = s.FileName,
                FileSize = s.FileSize,
                FileModifiedAt = s.FileModifiedAt,
                Duration = TimeSpan.FromMilliseconds(s.DurationMs),
                DiscNumber = s.DiscNumber,
                TrackNumber = s.TrackNumber,
                ReleaseYear = s.ReleaseYear,
                ReleaseDate = s.ReleaseDate,
                DateAdded = s.DateAdded,
                CoverPath = s.CoverPath,
                IsFavorite = s.IsFavorite,
                Rating = s.Rating
            })
            .ToListAsync(cancellationToken);

        foreach (var song in songs)
        {
            Songs.Add(song);
        }
    }

    private async Task LoadAlbumsAsync(CancellationToken cancellationToken)
    {
        Albums.Clear();

        var albums = await _dbContext.Songs
            .AsNoTracking()
            .Where(s => s.AlbumArtist == Artist.Name && !string.IsNullOrEmpty(s.Album))
            .GroupBy(s => new { s.Album, s.AlbumArtist })
            .Select(g => new Album
            {
                Title = g.Key.Album,
                Artist = string.IsNullOrEmpty(g.Key.AlbumArtist) ? "未知艺术家" : g.Key.AlbumArtist,
                SongCount = g.Count(),
                TotalDuration = TimeSpan.FromMilliseconds(g.Sum(s => s.DurationMs)),
                CoverPath = g.Where(s => s.CoverPath != null).Select(s => s.CoverPath).FirstOrDefault()
            })
            .OrderBy(a => a.Title)
            .ToListAsync(cancellationToken);

        foreach (var album in albums)
        {
            Albums.Add(album);
        }
    }

    private async Task LoadFeaturedAlbumsAsync(CancellationToken cancellationToken)
    {
        FeaturedAlbums.Clear();

        var featuredAlbums = await _dbContext.Songs
            .AsNoTracking()
            .Where(s => s.Artist == Artist.Name && s.AlbumArtist != Artist.Name && !string.IsNullOrEmpty(s.Album))
            .GroupBy(s => new { s.Album, s.AlbumArtist })
            .Select(g => new Album
            {
                Title = g.Key.Album,
                Artist = string.IsNullOrEmpty(g.Key.AlbumArtist) ? "未知艺术家" : g.Key.AlbumArtist,
                SongCount = g.Count(),
                TotalDuration = TimeSpan.FromMilliseconds(g.Sum(s => s.DurationMs)),
                CoverPath = g.Where(s => s.CoverPath != null).Select(s => s.CoverPath).FirstOrDefault()
            })
            .OrderBy(a => a.Title)
            .ToListAsync(cancellationToken);

        foreach (var album in featuredAlbums)
        {
            FeaturedAlbums.Add(album);
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
