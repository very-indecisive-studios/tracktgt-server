using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tracker.Service.Game;
using Tracker.Service.User;

namespace Tracker.Service.Test;

[TestClass]
public class UserServiceTest
{
    private IUserService _userService = new FirebaseAPIService("");

    [TestMethod]
    public async Task GetUserById()
    {
        // Setup
        
        // Execute
        var result = await _userService.GetUser("Vg8zoyfTHxNKoqyzPWkNpaOLtcz1");
        
        // Verify
        Assert.IsNotNull(result);
    }
}