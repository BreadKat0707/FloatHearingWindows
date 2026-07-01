using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using FloatHearing.Data;
using FloatHearing.Models;

namespace FloatHearing.ViewModels;

/// <summary>
/// 专辑详情页视图模型
/// </summary>
public sealed class AlbumDetailViewModel : INotifyPropertyChanged
{
    private readonly AppDbContext _dbContext;

    private Album _album = new();

    public Album Album
    {
        get => _album;
        set => SetProperty(ref _album, value);
    }

    public ObservableCollection<Song> Songs { get; } = [];

    public AlbumDetailViewModel(AppDbContext dbContext, Album? album = null)
    {
        _dbContext = dbContext;
        Album = album ?? new Album();
    }

    public async Task InitializeAsync(Album album, CancellationToken cancellationToken = default)
    {
        Album = album;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Album)));
        await LoadSongsAsync(cancellationToken);
    }

    public async Task LoadSongsAsync(CancellationToken cancellationToken = default)
    {
        Songs.Clear();

        var songs = await _dbContext.Songs
            .AsNoTracking()
            .Where(s => s.Album == Album.Title && s.AlbumArtist == Album.Artist)
            .OrderBy(s => s.DiscNumber ?? 0)
            .ThenBy(s => s.TrackNumber ?? 0)
            .ThenBy(s => s.Title)
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

        UpdateAlbumReleaseDateText();
    }

    private void UpdateAlbumReleaseDateText()
    {
        var releaseDate = Songs.Select(s => s.ReleaseDate).FirstOrDefault(d => d.HasValue);
        var releaseYear = Songs.Select(s => s.ReleaseYear).FirstOrDefault(y => y.HasValue);

        Album.ReleaseDateText = releaseDate.HasValue
            ? releaseDate.Value.ToString("yyyy-MM-dd")
            : releaseYear.HasValue
                ? releaseYear.Value.ToString()
                : null;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Album)));
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
