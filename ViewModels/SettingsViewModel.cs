using System.ComponentModel;
using System.Runtime.CompilerServices;
using FloatHearing.Data.Entities;
using FloatHearing.Services;

namespace FloatHearing.ViewModels;

/// <summary>
/// 设置页视图模型。
/// </summary>
public sealed class SettingsViewModel : INotifyPropertyChanged
{
    private readonly SettingsService _settingsService;

    public SettingsViewModel()
        : this(App.SettingsService)
    {
    }

    public SettingsViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
        _settingsService.PropertyChanged += SettingsService_PropertyChanged;
    }

    public IReadOnlyList<LanguageOption> AvailableLanguages => _settingsService.AvailableLanguages;

    public IReadOnlyList<ThemeModeOption> ThemeModeOptions { get; } = new List<ThemeModeOption>
    {
        new ThemeModeOption { Mode = ThemeMode.System, DisplayName = "跟随系统设置" },
        new ThemeModeOption { Mode = ThemeMode.Light, DisplayName = "浅色" },
        new ThemeModeOption { Mode = ThemeMode.Dark, DisplayName = "深色" }
    };

    public IReadOnlyList<BackdropMaterialOption> BackdropMaterialOptions { get; } = new List<BackdropMaterialOption>
    {
        new BackdropMaterialOption { Material = BackdropMaterial.Solid, DisplayName = "Solid" },
        new BackdropMaterialOption { Material = BackdropMaterial.Acrylic, DisplayName = "Acrylic" },
        new BackdropMaterialOption { Material = BackdropMaterial.Mica, DisplayName = "Mica" },
        new BackdropMaterialOption { Material = BackdropMaterial.MicaAlt, DisplayName = "Mica Alt" }
    };

    public ThemeModeOption SelectedThemeModeOption
    {
        get => ThemeModeOptions.First(o => o.Mode == _settingsService.ThemeMode);
        set
        {
            if (value is not null && _settingsService.ThemeMode != value.Mode)
            {
                _settingsService.ThemeMode = value.Mode;
                OnPropertyChanged();
            }
        }
    }

    public BackdropMaterialOption SelectedBackdropMaterialOption
    {
        get => BackdropMaterialOptions.First(o => o.Material == _settingsService.BackdropMaterial);
        set
        {
            if (value is not null && _settingsService.BackdropMaterial != value.Material)
            {
                _settingsService.BackdropMaterial = value.Material;
                OnPropertyChanged();
            }
        }
    }

    public string SelectedLanguage
    {
        get => _settingsService.Language;
        set
        {
            if (_settingsService.Language != value)
            {
                _settingsService.Language = value;
                OnPropertyChanged();
            }
        }
    }

    public async Task LoadAsync()
    {
        await _settingsService.LoadAsync();
        OnPropertyChanged(nameof(SelectedThemeModeOption));
        OnPropertyChanged(nameof(SelectedBackdropMaterialOption));
        OnPropertyChanged(nameof(SelectedLanguage));
    }

    private void SettingsService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SettingsService.ThemeMode))
        {
            OnPropertyChanged(nameof(SelectedThemeModeOption));
        }
        else if (e.PropertyName == nameof(SettingsService.BackdropMaterial))
        {
            OnPropertyChanged(nameof(SelectedBackdropMaterialOption));
        }
        else if (e.PropertyName == nameof(SettingsService.Language))
        {
            OnPropertyChanged(nameof(SelectedLanguage));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public sealed class ThemeModeOption
{
    public ThemeMode Mode { get; set; }

    public string DisplayName { get; set; } = string.Empty;
}

public sealed class BackdropMaterialOption
{
    public BackdropMaterial Material { get; set; }

    public string DisplayName { get; set; } = string.Empty;
}
