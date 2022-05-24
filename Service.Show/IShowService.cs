using Domain;

namespace Service.Show;

public interface IShowService
{
    Task<List<APIShow>> SearchShowByTitle(string title);

    Task<APIShow?> GetShowById(int id, ShowType showType);
}