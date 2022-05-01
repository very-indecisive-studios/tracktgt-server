using Microsoft.EntityFrameworkCore;
using Tracker.Persistence;

namespace Tracker.API.Extensions;

public static class DatabaseServiceExtensions
{
    public static void AddDatabaseServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<DatabaseContext>(options =>
        {
            options.UseSqlite(config.GetConnectionString("DefaultConnection"));
        });
    }
}