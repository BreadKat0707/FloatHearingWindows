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
/// 应用外壳视图模型，管理导航与全局播放状态。
/// </summary>
public sealed class ShellViewModel : INotifyPropertyChanged
{
    public PlaybackService Playback { get; }

    private object? _selectedNavigationItem;
    public object? SelectedNavigationItem
    {
        get => _selectedNavigationItem;
        set => SetProperty(ref _selectedNavigationItem, value);
    }

    public ShellViewModel(PlaybackService playback)
    {
        Playback = playback;
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
