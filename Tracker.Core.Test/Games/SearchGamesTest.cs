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
    private Mock<IGameService> _mockGameService { get; set; }

    private IMapper _mapper { get; set; }
    
    private SearchGamesHandler SearchGamesHandler { get; set; }
    
    [TestInitialize]
    public void TestClassInit()
    {
        _mockGameService = new Mock<IGameService>();

        var mappingConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile<MappingProfiles>();
        });
        _mapper = mappingConfig.CreateMapper();

        SearchGamesHandler = new SearchGamesHandler(_mockGameService.Object, _mapper);
    }

    [TestMethod]
    [DataRow("ch")]
    [DataRow("chaos")]
    [DataRow("chaos chef")]
    public async Task SearchGame_APIHit(string gameTitle)
    {
        var fakeAPIGameList = new List<APIGame>
        {
            new()
            {
                Id = 42069,
                Platforms = new List<string> { "PC" },
                Title = "Chaos Chef"
            },
            new()
            {
                Id = 12345,
                Platforms = new List<string> { "PC" },
                Title = "Chaos Chef Ultimate"
            }
        };
        
        _mockGameService
            .Setup(service => service.SearchGameByTitle(
                It.Is<string>(s => "chaos chef".Contains(s.ToLower()))))
            .ReturnsAsync(fakeAPIGameList);
        
        var result = await SearchGamesHandler.Handle(new SearchGamesQuery()
        {
            GameTitle = gameTitle
        }, CancellationToken.None);
        
        _mockGameService.Verify(service => service.SearchGameByTitle(gameTitle), Times.Once);
        Assert.AreEqual(2,result.Games.Count);
        Assert.IsNotNull(result.Games.Find(g => g.Id == fakeAPIGameList[0].Id));
        Assert.IsNotNull(result.Games.Find(g => g.Id == fakeAPIGameList[1].Id));
    }
    
    [TestMethod]
    [DataRow("sma")]
    [DataRow("smash balls")]
    [DataRow("risa_smash")]
    public async Task SearchGame_APINoHit(string gameTitle)
    {
        _mockGameService
            .Setup(service => service.SearchGameByTitle(It.IsAny<string>()))
            .ReturnsAsync(new List<APIGame>());
        
        var result = await SearchGamesHandler.Handle(new SearchGamesQuery()
        {
            GameTitle = gameTitle
        }, CancellationToken.None);
        
        _mockGameService.Verify(service => service.SearchGameByTitle(gameTitle), Times.Once);
        Assert.AreEqual(0,result.Games.Count);
    }
}