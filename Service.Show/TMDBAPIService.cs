using Domain;
using TMDbLib.Client;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.Search;
using TMDbLib.Objects.TvShows;

namespace Service.Show;

public class TMDBAPIService : IShowService
{
    public async Task<List<APIShow>> SearchShowByTitle(string title)
    {
        List<APIShow> list = new();
        
        TMDbClient client = new TMDbClient("APIKey");
        var searchContainer = await client.SearchMultiAsync(title);
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
                    movie.Overview,
                    ShowType.Movie));
            }
            else
            {
                SearchTv series = show as SearchTv;
                list.Add(new(
                    series.Id,
                    series.PosterPath,
                    series.Name,
                    series.Overview,
                    ShowType.Series));
            }
        }
        return list;
    }

    public async Task<APIShow?> GetShowById(int id, ShowType showType)
    {
        TMDbClient client = new TMDbClient("APIKey");

        if (showType == ShowType.Movie)
        {
            Movie movie = await client.GetMovieAsync(id);
            return new(
                movie.Id,
                movie.PosterPath,
                movie.Title,
                movie.Overview,
                ShowType.Movie);

        }
        else if (showType == ShowType.Series)
        {
            TvShow series = await client.GetTvShowAsync(id);
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