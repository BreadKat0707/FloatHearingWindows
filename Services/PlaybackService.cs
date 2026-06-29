using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using FloatHearing.Models;

namespace FloatHearing.Services;

/// <summary>
/// 应用级播放服务，管理 MediaPlayer、播放队列与播放状态。
/// </summary>
public sealed class PlaybackService : INotifyPropertyChanged
{
    private readonly MediaPlayer _mediaPlayer = new();
    private readonly DispatcherTimer _positionTimer;

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
    public double CurrentPosition
    {
        get => _currentPosition;
        set
        {
            if (SetProperty(ref _currentPosition, value))
            {
                if (_mediaPlayer.PlaybackSession != null)
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

    public PlaybackService()
    {
        _mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
        _mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
        _mediaPlayer.Volume = _volume;

        _positionTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(0.5)
        };
        _positionTimer.Tick += PositionTimer_Tick;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
    {
        _positionTimer.Start();
        Duration = sender.NaturalDuration != TimeSpan.Zero
            ? sender.NaturalDuration.TotalSeconds
            : 0;
    }

    private void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
    {
        _positionTimer.Stop();
        CurrentPosition = 0;
        IsPlaying = false;
    }

    private void PositionTimer_Tick(object? sender, object e)
    {
        if (_mediaPlayer.PlaybackSession != null)
        {
            CurrentPosition = _mediaPlayer.PlaybackSession.Position.TotalSeconds;
        }
    }

    public async void Play(Song song)
    {
        if (string.IsNullOrWhiteSpace(song.FilePath))
        {
            return;
        }

        if (CurrentSong?.Id != song.Id)
        {
            CurrentSong = song;
            var file = await StorageFile.GetFileFromPathAsync(song.FilePath);
            _mediaPlayer.Source = MediaSource.CreateFromStorageFile(file);
        }

        _mediaPlayer.Play();
        IsPlaying = true;
    }

    public void PlayPause()
    {
        if (_mediaPlayer.PlaybackSession?.PlaybackState == MediaPlaybackState.Playing)
        {
            _mediaPlayer.Pause();
            IsPlaying = false;
            _positionTimer.Stop();
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
