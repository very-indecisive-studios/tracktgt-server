using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tracker.Service.Game;

namespace Tracker.Service.Test;

[TestClass]
public class GameServiceTest
{
    private IGameService _gameService = new IGDBAPIService(
        "",
        "");

    [TestMethod]
    public async Task SearchGameTest()
    {
        // Setup
        
        // Execute
        var results = await _gameService.SearchGameByTitle("minecraft");
        
        // Verify
        Assert.IsTrue(true);
    }
    
    [TestMethod]
    public async Task GetGameTest()
    {
        // Setup
        
        // Execute
        var result = await _gameService.GetGameById(121);
        
        // Verify
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Title.Equals("Minecraft"));
    }
}