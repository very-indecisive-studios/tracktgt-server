using IGDBLib = IGDB;

namespace Tracker.Service.Game;

public class IGDBAPIService : IGameService
{
    private readonly IGDBLib.IGDBClient _client;

    public IGDBAPIService(string clientId, string clientSecret)
    {
        _client = new IGDBLib.IGDBClient(clientId, clientSecret);
    }

    public async Task<List<APIGame>> SearchGameByTitle(string title)
    {
        var result = await _client.QueryAsync<IGDBLib.Models.Game>(
            IGDBLib.IGDBClient.Endpoints.Games,
            $"search \"{title.ToLower()}\"; fields name,platforms.*;"
        );

        List<APIGame> list = new();
        
        foreach (var game in result)
        {
            if (game.Id != null && game.Platforms != null)
            {
                var platforms = new List<string>();
                foreach (var platform in game.Platforms.Values)
                {
                    platforms.Add(platform.Abbreviation ?? platform.Name);
                }
                
                list.Add(new()
                {
                    Id = game.Id.Value,
                    Title = game.Name,
                    Platforms = platforms
                });
            }
        }

        return list;
    }

    public async Task<APIGame?> GetGameById(long id)
    {
        // TODO: Complete query.
        var result = await _client.QueryAsync<IGDBLib.Models.Game>(
            IGDBLib.IGDBClient.Endpoints.Games,
            $"fields name,platforms.*,summary,total_rating,cover.*,involved_companies.company.*; where id = ({id});"
        );

        if (result.Length > 0)
        {
            var game = result[0];
            if (game.Id != null && game.Platforms != null)
            {
                var platforms = new List<string>();
                foreach (var platform in game.Platforms.Values)
                {
                    platforms.Add(platform.Abbreviation ?? platform.Name);
                }

                var coverURL = game.Cover != null ? "https:" + game.Cover.Value.Url : null;

                var companies = new List<string>();
                if (game.InvolvedCompanies != null)
                {
                    foreach (var company in game.InvolvedCompanies.Values)
                    {
                        companies.Add(company.Company.Value.Name);
                    }
                }
                
                return new APIGame()
                {
                    Id = game.Id.Value,
                    CoverImageURL = coverURL,
                    Title = game.Name,
                    Summary = game.Summary,
                    Rating = game.TotalRating ?? 0,
                    Platforms = platforms,
                    Companies = companies
                };
            }
        }

        return null; 
    }
}