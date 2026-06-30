using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using FloatHearing.Core.Models;
using FloatHearing.Data;
using FloatHearing.Models;
using FloatHearing.Services;

namespace FloatHearing.ViewModels;

/// <summary>
/// 歌曲页视图模型
/// </summary>
public sealed class SongsViewModel : INotifyPropertyChanged
{
    private readonly AppDbContext _dbContext;
    private readonly LibraryScanner _scanner;
    private readonly PlaybackService _playback;
    private readonly SettingsService _settingsService;
    private readonly SongContextMenuViewModel _contextMenu;

    public ObservableCollection<Song> Songs { get; } = [];

    public SongContextMenuViewModel ContextMenu => _contextMenu;

    private Song? _selectedSong;
    public Song? SelectedSong
    {
        get => _selectedSong;
        set
        {
            if (SetProperty(ref _selectedSong, value) && value is not null)
            {
                _playback.SetQueue(Songs);
                _playback.Play(value);
            }
        }
    }

    private bool _isScanning;
    public bool IsScanning
    {
        get => _isScanning;
        set => SetProperty(ref _isScanning, value);
    }

    private string _scanStatus = string.Empty;
    public string ScanStatus
    {
        get => _scanStatus;
        set => SetProperty(ref _scanStatus, value);
    }

    private SongSortField _currentSortField = SongSortField.Title;
    public SongSortField CurrentSortField
    {
        get => _currentSortField;
        set
        {
            if (SetProperty(ref _currentSortField, value))
            {
                _settingsService.SongSortField = value;
                _ = ReloadAndSortAsync();
            }
        }
    }

    private SortDirection _currentSortDirection = SortDirection.Ascending;
    public SortDirection CurrentSortDirection
    {
        get => _currentSortDirection;
        set
        {
            if (SetProperty(ref _currentSortDirection, value))
            {
                _settingsService.SortDirection = value;
                ApplySort();
            }
        }
    }

    public IReadOnlyList<SongSortField> AvailableSortFields { get; } =
        Enum.GetValues<SongSortField>().ToList();

    public IReadOnlyList<SongSortField> SortFields { get; } =
        Enum.GetValues<SongSortField>().ToList();

    public SongsViewModel(PlaybackService playback)
        : this(playback, App.SettingsService)
    {
    }

    public SongsViewModel(PlaybackService playback, SettingsService settingsService)
    {
        _dbContext = new AppDbContext
        {
            DbPath = App.DbContext.DbPath
        };
        _scanner = new LibraryScanner(_dbContext);
        _playback = playback;
        _settingsService = settingsService;
        _contextMenu = new SongContextMenuViewModel(_dbContext, playback);
    }

    public SongsViewModel(AppDbContext dbContext, PlaybackService playback, SettingsService settingsService)
    {
        _dbContext = dbContext;
        _scanner = new LibraryScanner(dbContext);
        _playback = playback;
        _settingsService = settingsService;
        _contextMenu = new SongContextMenuViewModel(dbContext, playback);
    }

    public async Task LoadLibraryAsync(CancellationToken cancellationToken = default)
    {
        Songs.Clear();

        var query = _dbContext.Songs
            .AsNoTracking()
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
                PlayCount = s.PlaybackStats != null ? s.PlaybackStats.PlayCount : 0,
                IsFavorite = s.IsFavorite,
                Rating = s.Rating
            });

        var songs = await query.ToListAsync(cancellationToken);

        foreach (var song in songs)
        {
            Songs.Add(song);
        }

        CurrentSortField = _settingsService.SongSortField;
        CurrentSortDirection = _settingsService.SortDirection;
        ApplySort();

        await _contextMenu.LoadPlaylistsAsync(cancellationToken);
    }

    public async Task ReloadAndSortAsync(CancellationToken cancellationToken = default)
    {
        await LoadLibraryAsync(cancellationToken);
    }

    public void ToggleSortDirection()
    {
        CurrentSortDirection = CurrentSortDirection == SortDirection.Ascending
            ? SortDirection.Descending
            : SortDirection.Ascending;
    }

    public void ApplySort()
    {
        var sorted = CurrentSortField switch
        {
            SongSortField.Title => Sort(Songs, s => s.Title, StringComparer.CurrentCultureIgnoreCase),
            SongSortField.AlbumDiscTrack => Sort(Songs, s => (s.Album, s.DiscNumber ?? 0, s.TrackNumber ?? 0)),
            SongSortField.Duration => Sort(Songs, s => s.Duration),
            SongSortField.FileSize => Sort(Songs, s => s.FileSize),
            SongSortField.FileModifiedAt => Sort(Songs, s => s.FileModifiedAt),
            SongSortField.DateAdded => Sort(Songs, s => s.DateAdded),
            SongSortField.FileName => Sort(Songs, s => s.FileName, StringComparer.CurrentCultureIgnoreCase),
            SongSortField.ArtistAlbum => Sort(Songs, s => (s.Artist, s.Album)),
            SongSortField.ReleaseDate => Sort(Songs, s => s.ReleaseDate, Comparer<DateTime?>.Default),
            SongSortField.PlayCount => Sort(Songs, s => s.PlayCount),
            _ => Songs.ToList()
        };

        var selected = SelectedSong;
        Songs.Clear();
        foreach (var song in sorted)
        {
            Songs.Add(song);
        }

        if (selected is not null)
        {
            SelectedSong = Songs.FirstOrDefault(s => s.Id == selected.Id);
        }
    }

    private List<Song> Sort<TKey>(IEnumerable<Song> source, Func<Song, TKey> keySelector, IComparer<TKey>? comparer = null)
    {
        var ordered = CurrentSortDirection == SortDirection.Ascending
            ? source.OrderBy(keySelector, comparer)
            : source.OrderByDescending(keySelector, comparer);
        return ordered.ToList();
    }

    public async Task ScanAsync(CancellationToken cancellationToken = default)
    {
        var paths = await _dbContext.ScanPaths
            .AsNoTracking()
            .Select(s => s.Path)
            .ToListAsync(cancellationToken);

        if (paths.Count == 0)
        {
            return;
        }

        IsScanning = true;
        ScanStatus = "准备扫描...";

        var progress = new Progress<ScanProgress>(p =>
        {
            ScanStatus = p.TotalCount > 0
                ? $"{p.Phase} ({p.ProcessedCount}/{p.TotalCount})"
                : p.Phase;
        });

        try
        {
            await _scanner.ScanAsync(paths, progress, cancellationToken);
            await LoadLibraryAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // 忽略取消
        }
        finally
        {
            IsScanning = false;
            ScanStatus = string.Empty;
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
