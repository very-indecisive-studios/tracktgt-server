using Core.Pricing;
using Service.Show;
using Service.Book;
using Service.Game;

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

        services.AddSingleton<IBookService>(new GoogleBooksAPIService(
            configuration["GoogleBooks:APIKey"]));

        services.AddSingleton<IGameMall, GameMall>();
    }
}