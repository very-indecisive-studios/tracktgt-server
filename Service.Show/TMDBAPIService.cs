using Domain;
using TMDbLib.Client;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.Search;
using TMDbLib.Objects.TvShows;

namespace Service.Show;

public class TMDBAPIService : IShowService
{
    private readonly TMDbClient _client;

    public TMDBAPIService(string apiKey)
    {
        _client = new TMDbClient(apiKey);
    }

    public async Task<List<APIShowBasic>> SearchShowByTitle(string title)
    {
        List<APIShowBasic> list = new();
        var searchContainer = await _client.SearchMultiAsync(title);
        var showList = searchContainer.Results
            .Where(sb => sb.MediaType != MediaType.Person)
            .ToList();

        foreach (var show in showList)
        {
            if (show.MediaType == MediaType.Movie)
            {
                SearchMovie movie = show as SearchMovie;
                list.Add(new(
                    movie.Id,
                    movie.PosterPath,
                    movie.Title,
                    ShowType.Movie));
            }
            else
            {
                SearchTv series = show as SearchTv;
                list.Add(new(
                    series.Id,
                    series.PosterPath,
                    series.Name,
                    ShowType.Series));
            }
        }
        return list;
    }

    public async Task<APIShow?> GetShowById(int id, ShowType showType)
    {
        if (showType == ShowType.Movie)
        {
            Movie movie = await _client.GetMovieAsync(id);
            return new(
                movie.Id,
                movie.PosterPath,
                movie.Title,
                movie.Overview,
                ShowType.Movie);

        }
        else if (showType == ShowType.Series)
        {
            TvShow series = await _client.GetTvShowAsync(id);
            return new(
                series.Id,
                series.PosterPath,
                series.Name,
                series.Overview,
                ShowType.Series);
        }
        return null;
    }
}