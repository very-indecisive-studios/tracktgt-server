using Microsoft.EntityFrameworkCore;
using Serilog;
using Tracker.Persistence;

namespace Tracker.API.Extensions;

public static class DatabaseHostExtensions
{
    public static async void ConfigDatabase(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<DatabaseContext>();
            await context.Database.MigrateAsync();
        }
        catch (Exception e)
        {
            Log.Logger.Error(e, "An error occured during migration!");
        }
    }
}