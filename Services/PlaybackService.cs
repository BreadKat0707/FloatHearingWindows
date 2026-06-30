using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
using FloatHearing.Data;
using FloatHearing.Data.Entities;
using FloatHearing.Models;

namespace FloatHearing.Services;

/// <summary>
/// 应用级播放服务，管理 MediaPlayer、播放队列与播放状态，并持久化到数据库。
/// </summary>
public sealed class PlaybackService : INotifyPropertyChanged
{
    private readonly AppDbContext _dbContext;
    private readonly MediaPlayer _mediaPlayer = new();
    private readonly SystemMediaTransportControls _smtc;
    private readonly DispatcherTimer _positionTimer;
    private readonly DispatcherTimer _saveStateTimer;
    private readonly Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue;

    public ObservableCollection<Song> Queue { get; } = [];

    private Song? _currentSong;
    public Song? CurrentSong
    {
        get => _currentSong;
        private set => SetProperty(ref _currentSong, value);
    }

    private bool _isPlaying;
    public bool IsPlaying
    {
        get => _isPlaying;
        private set => SetProperty(ref _isPlaying, value);
    }

    private double _currentPosition;
    private bool _isPositionUpdating;

    public double CurrentPosition
    {
        get => _currentPosition;
        set
        {
            if (SetProperty(ref _currentPosition, value))
            {
                if (_mediaPlayer.PlaybackSession != null && !_isPositionUpdating)
                {
                    _mediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(value);
                }
            }
        }
    }

    private double _duration;
    public double Duration
    {
        get => _duration;
        private set => SetProperty(ref _duration, value);
    }

    private double _volume = 0.8;
    public double Volume
    {
        get => _volume;
        set
        {
            if (SetProperty(ref _volume, value))
            {
                _mediaPlayer.Volume = value;
            }
        }
    }

    private int _repeatMode;
    public int RepeatMode
    {
        get => _repeatMode;
        set => SetProperty(ref _repeatMode, value);
    }

    private bool _shuffleMode;
    public bool ShuffleMode
    {
        get => _shuffleMode;
        set => SetProperty(ref _shuffleMode, value);
    }

    public PlaybackService()
        : this(App.DbContext)
    {
    }

    public PlaybackService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        SmtcLog("PlaybackService constructing");
        _mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
        _mediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
        _mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
        _mediaPlayer.PlaybackSession.NaturalDurationChanged += PlaybackSession_NaturalDurationChanged;
        _mediaPlayer.Volume = _volume;

        _smtc = _mediaPlayer.SystemMediaTransportControls;
        _smtc.IsEnabled = true;
        _smtc.IsPlayEnabled = true;
        _smtc.IsPauseEnabled = true;
        _smtc.IsNextEnabled = true;
        _smtc.IsPreviousEnabled = true;
        _smtc.ButtonPressed += Smtc_ButtonPressed;
        _smtc.PlaybackPositionChangeRequested += Smtc_PlaybackPositionChangeRequested;

        _mediaPlayer.CommandManager.NextBehavior.EnablingRule = MediaCommandEnablingRule.Always;
        _mediaPlayer.CommandManager.PreviousBehavior.EnablingRule = MediaCommandEnablingRule.Always;
        _mediaPlayer.CommandManager.PlayReceived += CommandManager_PlayReceived;
        _mediaPlayer.CommandManager.PauseReceived += CommandManager_PauseReceived;
        _mediaPlayer.CommandManager.NextReceived += CommandManager_NextReceived;
        _mediaPlayer.CommandManager.PreviousReceived += CommandManager_PreviousReceived;

        _positionTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(0.5)
        };
        _positionTimer.Tick += PositionTimer_Tick;

        _saveStateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5)
        };
        _saveStateTimer.Tick += SaveStateTimer_Tick;
        _saveStateTimer.Start();
    }

    /// <summary>
    /// 从数据库恢复播放状态。应在数据库初始化完成后调用。
    /// </summary>
    public async Task InitializeAsync()
    {
        await LoadStateAsync();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void PlaybackSession_NaturalDurationChanged(MediaPlaybackSession sender, object args)
    {
        if (sender.NaturalDuration != TimeSpan.Zero)
        {
            Duration = sender.NaturalDuration.TotalSeconds;
        }
    }

    private void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
    {
        SmtcLog("MediaPlayer.MediaOpened");
        _dispatcherQueue?.TryEnqueue(() =>
        {
            try
            {
                _positionTimer.Start();
                UpdateSmtcPlaybackStatus(MediaPlaybackStatus.Playing);
            }
            catch (Exception ex)
            {
                SmtcLog($"MediaOpened UI error: {ex}");
            }
        });
    }

    private void MediaPlayer_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
    {
        SmtcLog($"MediaPlayer.MediaFailed: Error={args.Error}, ErrorMessage={args.ErrorMessage}");
    }

    private void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
    {
        SmtcLog("MediaPlayer.MediaEnded");
        _dispatcherQueue?.TryEnqueue(async () =>
        {
            try
            {
                SmtcLog("MediaEnded UI handler start");
                _positionTimer.Stop();
                CurrentPosition = 0;
                IsPlaying = false;
                UpdateSmtcPlaybackStatus(MediaPlaybackStatus.Stopped);
                await SavePlaybackStatsAsync(true);

                if (Queue.Count > 0 && CurrentSong is not null)
                {
                    var index = Queue.IndexOf(CurrentSong);
                    var nextIndex = index < 0 || index >= Queue.Count - 1 ? 0 : index + 1;
                    SmtcLog($"MediaEnded: auto-playing next {nextIndex}");
                    Play(Queue[nextIndex]);
                }
                else
                {
                    _ = SaveStateAsync();
                }

                SmtcLog("MediaEnded UI handler end");
            }
            catch (Exception ex)
            {
                SmtcLog($"MediaEnded handler error: {ex}");
            }
        });
    }

    private void PositionTimer_Tick(object? sender, object e)
    {
        if (_mediaPlayer.PlaybackSession != null)
        {
            _isPositionUpdating = true;
            CurrentPosition = _mediaPlayer.PlaybackSession.Position.TotalSeconds;
            _isPositionUpdating = false;
            UpdateSmtcTimeline();
        }
    }

    private void SaveStateTimer_Tick(object? sender, object e)
    {
        _ = SaveStateAsync();
    }

    public async void Play(Song song)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(song.FilePath))
            {
                SmtcLog("Play skipped: empty file path");
                return;
            }

            SmtcLog($"Play called: {song.Title}");

            if (CurrentSong?.Id != song.Id)
            {
                if (CurrentSong is not null)
                {
                    await SavePlaybackStatsAsync(false);
                }

                CurrentSong = song;
                Duration = song.Duration.TotalSeconds;
                CurrentPosition = 0;
                var file = await StorageFile.GetFileFromPathAsync(song.FilePath);
                var source = MediaSource.CreateFromStorageFile(file);
                var playbackItem = new MediaPlaybackItem(source);
                playbackItem.Source.OpenOperationCompleted += (s, e) =>
                {
                    SmtcLog($"OpenOperationCompleted");
                };

                var displayProperties = playbackItem.GetDisplayProperties();
                displayProperties.Type = MediaPlaybackType.Music;
                displayProperties.MusicProperties.Title = song.Title;
                displayProperties.MusicProperties.Artist = song.Artist;
                displayProperties.MusicProperties.AlbumTitle = song.Album;

                if (!string.IsNullOrWhiteSpace(song.CoverPath))
                {
                    try
                    {
                        var coverUri = new Uri($"ms-appdata:///local/{song.CoverPath.Replace('\\', '/')}");
                        displayProperties.Thumbnail = RandomAccessStreamReference.CreateFromUri(coverUri);
                        SmtcLog($"Thumbnail set: {song.CoverPath}");
                    }
                    catch (Exception ex)
                    {
                        SmtcLog($"Thumbnail load failed: {ex.Message}");
                    }
                }

                playbackItem.ApplyDisplayProperties(displayProperties);
                _mediaPlayer.Source = playbackItem;
                SmtcLog("MediaPlaybackItem source set");
            }

            await UpdateSmtcInfoCoreAsync();
            _mediaPlayer.Play();
            IsPlaying = true;
            UpdateSmtcPlaybackStatus(MediaPlaybackStatus.Playing);
            UpdateSmtcTimeline();
            _positionTimer.Start();
            SmtcLog("Play finished");
        }
        catch (Exception ex)
        {
            SmtcLog($"Play error: {ex}");
        }
    }

    public void PlayPause()
    {
        if (_mediaPlayer.PlaybackSession?.PlaybackState == MediaPlaybackState.Playing)
        {
            _mediaPlayer.Pause();
            IsPlaying = false;
            UpdateSmtcPlaybackStatus(MediaPlaybackStatus.Paused);
            _positionTimer.Stop();
            _ = SaveStateAsync();
        }
        else
        {
            if (_mediaPlayer.Source is null && CurrentSong is not null)
            {
                Play(CurrentSong);
            }
            else
            {
                _mediaPlayer.Play();
                IsPlaying = true;
                UpdateSmtcPlaybackStatus(MediaPlaybackStatus.Playing);
                _positionTimer.Start();
            }
        }
    }

    public void Previous()
    {
        if (Queue.Count == 0 || CurrentSong is null)
        {
            return;
        }

        var index = Queue.IndexOf(CurrentSong);
        var previousIndex = index <= 0 ? Queue.Count - 1 : index - 1;
        Play(Queue[previousIndex]);
    }

    public void Next()
    {
        if (Queue.Count == 0 || CurrentSong is null)
        {
            return;
        }

        var index = Queue.IndexOf(CurrentSong);
        var nextIndex = index < 0 || index >= Queue.Count - 1 ? 0 : index + 1;
        Play(Queue[nextIndex]);
    }

    public void SetQueue(IEnumerable<Song> songs)
    {
        Queue.Clear();
        foreach (var song in songs)
        {
            Queue.Add(song);
        }
    }

    private void CommandManager_PlayReceived(MediaPlaybackCommandManager sender, MediaPlaybackCommandManagerPlayReceivedEventArgs args)
    {
        args.Handled = true;
        Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() => PlayPause());
    }

    private void CommandManager_PauseReceived(MediaPlaybackCommandManager sender, MediaPlaybackCommandManagerPauseReceivedEventArgs args)
    {
        args.Handled = true;
        Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() => PlayPause());
    }

    private void CommandManager_NextReceived(MediaPlaybackCommandManager sender, MediaPlaybackCommandManagerNextReceivedEventArgs args)
    {
        args.Handled = true;
        Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() => Next());
    }

    private void CommandManager_PreviousReceived(MediaPlaybackCommandManager sender, MediaPlaybackCommandManagerPreviousReceivedEventArgs args)
    {
        args.Handled = true;
        Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() => Previous());
    }

    private void Smtc_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
    {
        _ = args.Button switch
        {
            SystemMediaTransportControlsButton.Play => Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() => PlayPause()),
            SystemMediaTransportControlsButton.Pause => Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() => PlayPause()),
            SystemMediaTransportControlsButton.Next => Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() => Next()),
            SystemMediaTransportControlsButton.Previous => Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() => Previous()),
            _ => true
        };
    }

    private void Smtc_PlaybackPositionChangeRequested(SystemMediaTransportControls sender, PlaybackPositionChangeRequestedEventArgs args)
    {
        var position = args.RequestedPlaybackPosition.TotalSeconds;
        if (position >= 0 && position <= Duration)
        {
            Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
            {
                CurrentPosition = position;
            });
        }
    }

    private void UpdateSmtcPlaybackStatus(MediaPlaybackStatus status)
    {
        try
        {
            _smtc.PlaybackStatus = status;
            UpdateSmtcTimeline();
        }
        catch
        {
            // 忽略 SMTC 更新失败
        }
    }

    private void UpdateSmtcInfo()
    {
        SmtcLog("UpdateSmtcInfo called");
        _ = UpdateSmtcInfoCoreAsync();
    }

    private async Task UpdateSmtcInfoCoreAsync()
    {
        try
        {
            var updater = _smtc.DisplayUpdater;
            var song = CurrentSong;
            if (song is null)
            {
                updater.ClearAll();
                updater.Update();
                SmtcLog("Cleared SMTC info");
                return;
            }

            updater.Type = MediaPlaybackType.Music;
            updater.MusicProperties.Title = song.Title;
            updater.MusicProperties.Artist = song.Artist;
            updater.MusicProperties.AlbumTitle = song.Album;
            updater.AppMediaId = $"FloatHearing-{song.Id}";

            SmtcLog($"Updating SMTC: Title={song.Title}, Artist={song.Artist}, Album={song.Album}");

            if (!string.IsNullOrWhiteSpace(song.CoverPath))
            {
                try
                {
                    var coverUri = new Uri($"ms-appdata:///local/{song.CoverPath.Replace('\\', '/')}");
                    updater.Thumbnail = RandomAccessStreamReference.CreateFromUri(coverUri);
                    SmtcLog($"Set thumbnail: {song.CoverPath}");
                }
                catch (Exception ex)
                {
                    updater.Thumbnail = null;
                    SmtcLog($"Thumbnail load failed: {ex.Message}");
                }
            }
            else
            {
                updater.Thumbnail = null;
                SmtcLog("No cover path");
            }

            updater.Update();
            SmtcLog("SMTC info updated successfully");
        }
        catch (Exception ex)
        {
            SmtcLog($"SMTC update failed: {ex}");
        }
    }

    private static string LogPath
    {
        get
        {
            try
            {
                return Path.Combine(ApplicationData.Current.LocalFolder.Path, "smtc.log");
            }
            catch
            {
                try
                {
                    var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    return Path.Combine(appData, "FloatHearing", "smtc.log");
                }
                catch
                {
                    return Path.Combine(Path.GetTempPath(), "floathearing_smtc.log");
                }
            }
        }
    }

    private static void SmtcLog(string message)
    {
        try
        {
            var logPath = LogPath;
            var line = $"[{DateTime.UtcNow:O}] {message}{Environment.NewLine}";
            File.AppendAllText(logPath, line);
        }
        catch
        {
            // 忽略日志写入失败
        }
    }

    private void UpdateSmtcTimeline()
    {
        try
        {
            if (Duration <= 0)
            {
                return;
            }

            var timelineProperties = new SystemMediaTransportControlsTimelineProperties
            {
                StartTime = TimeSpan.Zero,
                EndTime = TimeSpan.FromSeconds(Duration),
                MinSeekTime = TimeSpan.Zero,
                MaxSeekTime = TimeSpan.FromSeconds(Duration),
                Position = TimeSpan.FromSeconds(CurrentPosition)
            };

            _smtc.UpdateTimelineProperties(timelineProperties);
        }
        catch
        {
            // 忽略时间线更新失败
        }
    }

    public async Task LoadStateAsync()
    {
        var state = await _dbContext.PlaybackStates
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (state is null)
        {
            return;
        }

        Volume = state.Volume > 0 ? state.Volume : 0.8;
        RepeatMode = state.RepeatMode;
        ShuffleMode = state.ShuffleMode;

        if (!string.IsNullOrWhiteSpace(state.QueueJson))
        {
            try
            {
                var queueIds = JsonSerializer.Deserialize<List<long>>(state.QueueJson) ?? [];
                var songs = await _dbContext.Songs
                    .AsNoTracking()
                    .Where(s => queueIds.Contains(s.Id))
                    .ToListAsync();

                var ordered = queueIds
                    .Select(id => songs.FirstOrDefault(s => s.Id == id))
                    .Where(s => s is not null)
                    .Select(s => MapToSongModel(s!))
                    .ToList();

                Queue.Clear();
                foreach (var song in ordered)
                {
                    Queue.Add(song);
                }

                if (state.CurrentSongId.HasValue)
                {
                    var current = Queue.FirstOrDefault(s => s.Id == state.CurrentSongId.Value);
                    if (current is not null)
                    {
                        CurrentSong = current;
                        CurrentPosition = state.PositionMs / 1000.0;
                        if (_mediaPlayer.PlaybackSession is not null)
                        {
                            _mediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(CurrentPosition);
                        }
                    }
                }
            }
            catch
            {
                // 忽略恢复失败
            }
        }
    }

    public async Task SaveStateAsync()
    {
        try
        {
            var queueIds = Queue.Select(s => s.Id).ToList();
            var state = await _dbContext.PlaybackStates.FirstOrDefaultAsync()
                        ?? new PlaybackStateEntity { Id = 1 };

            state.CurrentSongId = CurrentSong?.Id;
            state.PositionMs = (long)(CurrentPosition * 1000);
            state.QueueJson = JsonSerializer.Serialize(queueIds);
            state.RepeatMode = RepeatMode;
            state.ShuffleMode = ShuffleMode;
            state.Volume = Volume;
            state.UpdatedAt = DateTime.UtcNow;

            if (state.Id == 0)
            {
                state.Id = 1;
                _dbContext.PlaybackStates.Add(state);
            }
            else
            {
                _dbContext.PlaybackStates.Update(state);
            }

            await _dbContext.SaveChangesAsync();
        }
        catch
        {
            // 忽略保存失败
        }
    }

    public async Task SavePlaybackStatsAsync(bool completed)
    {
        if (CurrentSong is null)
        {
            return;
        }

        try
        {
            var stats = await _dbContext.PlaybackStats
                .FirstOrDefaultAsync(s => s.SongId == CurrentSong.Id);

            if (stats is null)
            {
                stats = new PlaybackStatsEntity
                {
                    SongId = CurrentSong.Id,
                    PlayCount = 0,
                    TotalPlayedMs = 0,
                    SkipCount = 0
                };
                _dbContext.PlaybackStats.Add(stats);
            }
            else
            {
                _dbContext.PlaybackStats.Update(stats);
            }

            stats.PlayCount++;
            stats.TotalPlayedMs += (long)(CurrentPosition * 1000);
            stats.LastPlayedAt = DateTime.UtcNow;
            stats.UpdatedAt = DateTime.UtcNow;

            if (!completed)
            {
                stats.SkipCount++;
            }

            await _dbContext.SaveChangesAsync();
        }
        catch
        {
            // 忽略保存失败
        }
    }

    public async Task UpdateSongFavoriteAsync(long songId, bool isFavorite)
    {
        var entity = await _dbContext.Songs.FindAsync(songId);
        if (entity is null)
        {
            return;
        }

        entity.IsFavorite = isFavorite;
        entity.DateModified = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateSongRatingAsync(long songId, int rating)
    {
        var entity = await _dbContext.Songs.FindAsync(songId);
        if (entity is null)
        {
            return;
        }

        entity.Rating = Math.Clamp(rating, 0, 5);
        entity.DateModified = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
    }

    private static Song MapToSongModel(SongEntity entity)
    {
        return new Song
        {
            Id = entity.Id,
            Title = entity.Title,
            Artist = entity.Artist,
            Album = entity.Album,
            AlbumArtist = entity.AlbumArtist,
            FilePath = entity.FilePath,
            FileName = entity.FileName,
            FileSize = entity.FileSize,
            FileModifiedAt = entity.FileModifiedAt,
            Duration = TimeSpan.FromMilliseconds(entity.DurationMs),
            DiscNumber = entity.DiscNumber,
            TrackNumber = entity.TrackNumber,
            ReleaseYear = entity.ReleaseYear,
            ReleaseDate = entity.ReleaseDate,
            DateAdded = entity.DateAdded,
            CoverPath = entity.CoverPath,
            IsFavorite = entity.IsFavorite,
            Rating = entity.Rating,
            PlayCount = entity.PlaybackStats?.PlayCount ?? 0
        };
    }

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
