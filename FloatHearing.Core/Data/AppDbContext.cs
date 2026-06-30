using Microsoft.EntityFrameworkCore;
using FloatHearing.Data.Entities;

namespace FloatHearing.Data;

/// <summary>
/// 应用数据库上下文
/// </summary>
public sealed class AppDbContext : DbContext
{
    public DbSet<SongEntity> Songs => Set<SongEntity>();

    public DbSet<PlaylistEntity> Playlists => Set<PlaylistEntity>();

    public DbSet<PlaylistSongEntity> PlaylistSongs => Set<PlaylistSongEntity>();

    public DbSet<ScanPathEntity> ScanPaths => Set<ScanPathEntity>();

    public DbSet<PlaybackStateEntity> PlaybackStates => Set<PlaybackStateEntity>();

    public DbSet<PlaybackStatsEntity> PlaybackStats => Set<PlaybackStatsEntity>();

    public DbSet<InspirationNoteEntity> InspirationNotes => Set<InspirationNoteEntity>();

    public DbSet<HiddenFolderEntity> HiddenFolders => Set<HiddenFolderEntity>();

    public DbSet<AppSettingEntity> AppSettings => Set<AppSettingEntity>();

    /// <summary>
    /// 数据库文件路径。必须在 <see cref="OnConfiguring"/> 前设置。
    /// </summary>
    public string DbPath { get; set; } = string.Empty;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var path = string.IsNullOrWhiteSpace(DbPath)
                ? Path.Combine(Path.GetTempPath(), "floathearing.db")
                : DbPath;
            optionsBuilder.UseSqlite($"Data Source={path}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SongEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FilePath).IsUnique();
            entity.HasIndex(e => e.Title);
            entity.HasIndex(e => e.Artist);
            entity.HasIndex(e => e.Album);
            entity.HasOne(e => e.PlaybackStats)
                  .WithOne(e => e.Song)
                  .HasForeignKey<PlaybackStatsEntity>(e => e.SongId);
        });

        modelBuilder.Entity<PlaylistEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.IsSystem);
        });

        modelBuilder.Entity<PlaylistSongEntity>(entity =>
        {
            entity.HasKey(e => new { e.PlaylistId, e.SongId });
            entity.HasOne(e => e.Playlist)
                  .WithMany(e => e.PlaylistSongs)
                  .HasForeignKey(e => e.PlaylistId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Song)
                  .WithMany(e => e.PlaylistSongs)
                  .HasForeignKey(e => e.SongId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ScanPathEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Path).IsUnique();
        });

        modelBuilder.Entity<PlaybackStateEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<PlaybackStatsEntity>(entity =>
        {
            entity.HasKey(e => e.SongId);
        });

        modelBuilder.Entity<InspirationNoteEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SongId);
            entity.HasOne(e => e.Song)
                  .WithMany(e => e.InspirationNotes)
                  .HasForeignKey(e => e.SongId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HiddenFolderEntity>(entity =>
        {
            entity.HasKey(e => e.Path);
        });

        modelBuilder.Entity<AppSettingEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }
}
