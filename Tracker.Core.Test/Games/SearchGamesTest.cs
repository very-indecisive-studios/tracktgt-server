using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tracker.Core;
using Tracker.Core.Games;
using Tracker.Service.Game;

namespace Tracker.Core.Test.Games;

[TestClass]
public class SearchGamesTest
{
    private Mock<IGameService>? MockGameService { get; set; }

    private IMapper? Mapper { get; set; }
    
    private SearchGamesHandler? SearchGamesHandler { get; set; }
    
    [TestInitialize]
    public void TestCaseInit()
    {
        MockGameService = new Mock<IGameService>();

        var mappingConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile<MappingProfiles>();
        });
        Mapper = mappingConfig.CreateMapper();

        SearchGamesHandler = new SearchGamesHandler(MockGameService.Object, Mapper);
    }

    [TestMethod]
    [DataRow("ch")]
    [DataRow("chaos")]
    [DataRow("chaos chef")]
    public async Task SearchGame_APIHit(string gameTitle)
    {
        var fakeAPIGameList = new List<APIGameBasic>
        {
            new(42069, "http://image.example.com", "Chaos Chef", new List<string> { "PC" }),
            new(12345, "http://image2.example.com", "Chaos Chef Ultimate", new List<string> { "PC", "PS5" })
        };
        
        MockGameService!
            .Setup(service => service.SearchGameByTitle(
                It.Is<string>(s => "chaos chef".Contains(s.ToLower()))))
            .ReturnsAsync(fakeAPIGameList);
        
        var result = await SearchGamesHandler!.Handle(new SearchGamesQuery(gameTitle), CancellationToken.None);
        
        MockGameService.Verify(service => service.SearchGameByTitle(gameTitle), Times.Once);
        Assert.AreEqual(2,result.Games.Count);
        Assert.IsNotNull(result.Games.Find(g => g.RemoteId == fakeAPIGameList[0].Id));
        Assert.IsNotNull(result.Games.Find(g => g.RemoteId == fakeAPIGameList[1].Id));
    }
    
    [TestMethod]
    [DataRow("sma")]
    [DataRow("smash balls")]
    [DataRow("risa_smash")]
    public async Task SearchGame_APINoHit(string gameTitle)
    {
        MockGameService!
            .Setup(service => service.SearchGameByTitle(It.IsAny<string>()))
            .ReturnsAsync(new List<APIGameBasic>());
        
        var result = await SearchGamesHandler!.Handle(new SearchGamesQuery(gameTitle), CancellationToken.None);
        
        MockGameService.Verify(service => service.SearchGameByTitle(gameTitle), Times.Once);
        Assert.AreEqual(0,result.Games.Count);
    }
}