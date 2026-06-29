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

        var artists = await _dbContext.Songs
            .AsNoTracking()
            .Where(s => !string.IsNullOrEmpty(s.Artist))
            .GroupBy(s => s.Artist)
            .Select(g => new Artist
            {
                Name = g.Key,
                SongCount = g.Count(),
                AlbumCount = g.Select(s => s.Album).Distinct().Count(),
                TotalDuration = TimeSpan.FromMilliseconds(g.Sum(s => s.DurationMs))
            })
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);

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
