using Microsoft.EntityFrameworkCore;
using Domain;
using Domain.Pricing;

namespace Persistence;

#nullable disable

public class DatabaseContext : DbContext
{
    public virtual DbSet<User> Users { get; set; }
    
    public virtual DbSet<Game> Games { get; set; }
    
    public virtual DbSet<Book> Books { get; set; }
    
    public virtual DbSet<Follow> Follows { get; set; }
    
    public virtual DbSet<GameTracking> GameTrackings { get; set; }
    
    public virtual DbSet<GameWishlist> GameWishlists { get; set; }
    
    public virtual DbSet<BookTracking> BookTrackings { get; set; }
    
    public virtual DbSet<BookWishlist> BookWishlists { get; set; }

    public virtual DbSet<Show> Shows { get; set; }
    
    public virtual DbSet<ShowTracking> ShowTrackings { get; set; }
    
    public virtual DbSet<GamePrice> GamePrices { get; set; }
    
    public virtual DbSet<GameStoreMetadata> GameStoreMetadatas { get; set; }

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