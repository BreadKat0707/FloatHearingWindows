using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using FloatHearing.Data;
using FloatHearing.Data.Entities;
using FloatHearing.Models;
using FloatHearing.Services;

namespace FloatHearing.ViewModels;

/// <summary>
/// 歌曲右键菜单命令上下文。
/// </summary>
public sealed class SongContextMenuViewModel : INotifyPropertyChanged
{
    private readonly AppDbContext _dbContext;
    private readonly PlaybackService _playback;
    private Song? _targetSong;
    private IList<Song>? _sourceSongs;

    public ObservableCollection<PlaylistEntity> Playlists { get; } = [];

    public Song? TargetSong
    {
        get => _targetSong;
        set => SetProperty(ref _targetSong, value);
    }

    public SongContextMenuViewModel()
        : this(App.DbContext, App.PlaybackService)
    {
    }

    public SongContextMenuViewModel(AppDbContext dbContext, PlaybackService playback)
    {
        _dbContext = dbContext;
        _playback = playback;
    }

    public void SetContext(Song target, IList<Song> sourceSongs)
    {
        TargetSong = target;
        _sourceSongs = sourceSongs;
    }

    public async Task LoadPlaylistsAsync(CancellationToken cancellationToken = default)
    {
        Playlists.Clear();
        var playlists = await _dbContext.Playlists
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        foreach (var playlist in playlists)
        {
            Playlists.Add(playlist);
        }
    }

    public void PlayNow()
    {
        if (TargetSong is null)
        {
            return;
        }

        _playback.SetQueue(_sourceSongs ?? new List<Song> { TargetSong });
        _playback.Play(TargetSong);
    }

    public void PlayNext()
    {
        if (TargetSong is null)
        {
            return;
        }

        var queue = _playback.Queue;
        var current = _playback.CurrentSong;
        var index = current is null ? -1 : queue.IndexOf(current);
        var insertIndex = index < 0 ? queue.Count : index + 1;
        queue.Insert(insertIndex, TargetSong);
    }

    public async Task AddToPlaylistAsync(long playlistId)
    {
        if (TargetSong is null)
        {
            return;
        }

        var exists = await _dbContext.PlaylistSongs
            .AnyAsync(ps => ps.PlaylistId == playlistId && ps.SongId == TargetSong.Id);

        if (exists)
        {
            return;
        }

        _dbContext.PlaylistSongs.Add(new PlaylistSongEntity
        {
            PlaylistId = playlistId,
            SongId = TargetSong.Id,
            AddedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync();
    }

    public async Task RemoveFromLibraryAsync()
    {
        if (TargetSong is null)
        {
            return;
        }

        var entity = await _dbContext.Songs.FindAsync(TargetSong.Id);
        if (entity is null)
        {
            return;
        }

        _dbContext.Songs.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RescanMetadataAsync()
    {
        if (TargetSong is null || !File.Exists(TargetSong.FilePath))
        {
            return;
        }

        var scanner = new LibraryScanner(_dbContext);
        var fileInfo = new FileInfo(TargetSong.FilePath);
        var existing = await _dbContext.Songs.FindAsync(TargetSong.Id);
        var updated = await scanner.RescanSingleAsync(TargetSong.FilePath, fileInfo, existing);
        if (updated is not null)
        {
            await _dbContext.SaveChangesAsync();
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
