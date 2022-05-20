using Microsoft.EntityFrameworkCore;
using Persistence;

namespace API.Extensions;

public static class DatabaseServiceExtensions
{
    public static void AddDatabaseServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<DatabaseContext>(options =>
        {
            var connectionString = config.GetConnectionString("DefaultConnection");
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        });
    }
}