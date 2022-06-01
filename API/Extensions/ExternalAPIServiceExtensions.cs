using Service.Game;
using Service.Show;

namespace API.Extensions;

public static class ExternalAPIServiceExtentions
{
    public static void AddExternalAPIServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IGameService>(new IGDBAPIService(
            configuration["IGDB:ClientId"], 
            configuration["IGDB:ClientSecret"]));
        
        services.AddSingleton<IShowService>(new TMDBAPIService(
            configuration["TMDB:APIKey"]));
    }
}