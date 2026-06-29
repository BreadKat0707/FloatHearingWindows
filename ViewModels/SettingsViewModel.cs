using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using FloatHearing.Data;
using FloatHearing.Data.Entities;
using FloatHearing.Services;

namespace FloatHearing.ViewModels;

/// <summary>
/// 扫描目录管理页视图模型
/// </summary>
public sealed class SettingsViewModel : INotifyPropertyChanged
{
    private readonly AppDbContext _dbContext;
    private readonly LibraryScanner _scanner;

    public ObservableCollection<ScanPathEntity> ScanPaths { get; } = [];

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

    public SettingsViewModel(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        _scanner = new LibraryScanner(dbContext);
    }

    public async Task LoadScanPathsAsync(CancellationToken cancellationToken = default)
    {
        ScanPaths.Clear();

        var paths = await _dbContext.ScanPaths
            .AsNoTracking()
            .OrderBy(s => s.Path)
            .ToListAsync(cancellationToken);

        foreach (var path in paths)
        {
            ScanPaths.Add(path);
        }
    }

    public async Task AddScanPathAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        var normalized = Path.GetFullPath(path);
        if (await _dbContext.ScanPaths.AnyAsync(s => s.Path == normalized))
        {
            return;
        }

        _dbContext.ScanPaths.Add(new ScanPathEntity
        {
            Path = normalized,
            CreatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync();
        await LoadScanPathsAsync();
    }

    public async Task RemoveScanPathAsync(long id)
    {
        var entity = await _dbContext.ScanPaths.FindAsync(id);
        if (entity is null)
        {
            return;
        }

        _dbContext.ScanPaths.Remove(entity);
        await _dbContext.SaveChangesAsync();
        await LoadScanPathsAsync();
    }

    public async Task ScanAllAsync(CancellationToken cancellationToken = default)
    {
        var paths = await _dbContext.ScanPaths
            .AsNoTracking()
            .Select(s => s.Path)
            .ToListAsync(cancellationToken);

        if (paths.Count == 0)
        {
            ScanStatus = "没有已添加的扫描目录";
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
