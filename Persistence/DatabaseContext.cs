using Microsoft.EntityFrameworkCore;
using Domain;

namespace Persistence;

#nullable disable

public class DatabaseContext : DbContext
{
    public virtual DbSet<User> Users { get; set; }
    
    public virtual DbSet<Game> Games { get; set; }
    
    public virtual DbSet<GameTracking> GameTrackings { get; set; }

    public DatabaseContext() { }
    
    public DatabaseContext(DbContextOptions<DatabaseContext> options): base(options) { }
    
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