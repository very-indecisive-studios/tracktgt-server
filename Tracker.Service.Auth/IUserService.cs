namespace Tracker.Service.User;

public interface IUserService
{
    public Task<APIUser?> GetUser(string id);
}