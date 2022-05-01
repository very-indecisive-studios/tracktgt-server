using Microsoft.EntityFrameworkCore;
using Tracker.Domain;

namespace Tracker.Persistence;

public class DatabaseContext : DbContext
{
    public virtual DbSet<User> Users { get; set; }
    
    public virtual DbSet<Game> Games { get; set; }
    
    public virtual DbSet<TrackedGame> TrackedGames { get; set; }

    public DatabaseContext() { }
    
    public DatabaseContext(DbContextOptions<DatabaseContext> options): base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbFile = Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData), "tracktgt.db");

        optionsBuilder.UseSqlite($"Data Source={dbFile}");
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }
        
    private void UpdateAuditFields()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedOn = now;
                    entry.Entity.LastModifiedOn = now;
                    break;
                case EntityState.Modified:
                    entry.Entity.LastModifiedOn = now;
                    break;
            }
        }
    }
}