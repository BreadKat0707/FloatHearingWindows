using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using FloatHearing.Data;
using FloatHearing.Models;

namespace FloatHearing.ViewModels;

/// <summary>
/// 专辑页视图模型
/// </summary>
public sealed class AlbumsViewModel : INotifyPropertyChanged
{
    private readonly AppDbContext _dbContext;

    public ObservableCollection<Album> Albums { get; } = [];

    public AlbumsViewModel(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task LoadAlbumsAsync(CancellationToken cancellationToken = default)
    {
        Albums.Clear();

        var albums = await _dbContext.Songs
            .AsNoTracking()
            .Where(s => !string.IsNullOrEmpty(s.Album))
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
