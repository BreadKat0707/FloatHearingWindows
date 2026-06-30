using System.ComponentModel;
using System.Runtime.CompilerServices;
using FloatHearing.Core.Models;
using FloatHearing.Data;
using FloatHearing.Data.Entities;

namespace FloatHearing.Services;

/// <summary>
/// 应用设置服务，管理并持久化用户偏好设置。
/// </summary>
public sealed class SettingsService : INotifyPropertyChanged
{
    private readonly AppDbContext _dbContext;
    private AppSettingEntity _settings = new();

    public SettingsService()
        : this(App.DbContext)
    {
    }

    public SettingsService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 从数据库加载设置。
    /// </summary>
    public async Task LoadAsync()
    {
        var entity = await _dbContext.AppSettings.FindAsync(1);
        if (entity is null)
        {
            entity = new AppSettingEntity
            {
                Id = 1,
                Language = string.Empty
            };
            _dbContext.AppSettings.Add(entity);
            await _dbContext.SaveChangesAsync();
        }

        _settings = entity;
        OnPropertyChanged(nameof(ThemeMode));
        OnPropertyChanged(nameof(BackdropMaterial));
        OnPropertyChanged(nameof(Language));
        OnPropertyChanged(nameof(AvailableLanguages));
        OnPropertyChanged(nameof(SongSortField));
        OnPropertyChanged(nameof(SortDirection));
    }

    /// <summary>
    /// 可用语言列表。
    /// </summary>
    public IReadOnlyList<LanguageOption> AvailableLanguages { get; } = new List<LanguageOption>
    {
        new LanguageOption { Tag = "", DisplayName = "跟随系统" },
        new LanguageOption { Tag = "zh-CN", DisplayName = "简体中文" },
        new LanguageOption { Tag = "zh-TW", DisplayName = "繁體中文" },
        new LanguageOption { Tag = "en-US", DisplayName = "English" },
        new LanguageOption { Tag = "ja-JP", DisplayName = "日本語" }
    };

    public ThemeMode ThemeMode
    {
        get => _settings.ThemeMode;
        set
        {
            if (_settings.ThemeMode != value)
            {
                _settings.ThemeMode = value;
                _ = SaveAsync();
                OnPropertyChanged();
            }
        }
    }

    public BackdropMaterial BackdropMaterial
    {
        get => _settings.BackdropMaterial;
        set
        {
            if (_settings.BackdropMaterial != value)
            {
                _settings.BackdropMaterial = value;
                _ = SaveAsync();
                OnPropertyChanged();
            }
        }
    }

    public string Language
    {
        get => _settings.Language;
        set
        {
            if (_settings.Language != value)
            {
                _settings.Language = value;
                _ = SaveAsync();
                OnPropertyChanged();
            }
        }
    }

    public SongSortField SongSortField
    {
        get => (SongSortField)_settings.SongSortField;
        set
        {
            var raw = (int)value;
            if (_settings.SongSortField != raw)
            {
                _settings.SongSortField = raw;
                _ = SaveAsync();
                OnPropertyChanged();
            }
        }
    }

    public SortDirection SortDirection
    {
        get => (SortDirection)_settings.SortDirection;
        set
        {
            var raw = (int)value;
            if (_settings.SortDirection != raw)
            {
                _settings.SortDirection = raw;
                _ = SaveAsync();
                OnPropertyChanged();
            }
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch
        {
            // 持久化失败时忽略，避免崩溃。
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// 语言选项。
/// </summary>
public sealed class LanguageOption
{
    public string Tag { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;
}
