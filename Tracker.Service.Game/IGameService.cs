namespace Tracker.Service.Game;

public interface IGameService
{
    Task<List<APIGame>> SearchGameByTitle(string title);

    Task<APIGame?> GetGameById(long id);
}
