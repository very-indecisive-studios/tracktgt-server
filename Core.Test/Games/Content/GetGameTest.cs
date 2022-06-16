using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Exceptions;
using Core.Games.Content;
using Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.EntityFrameworkCore;
using Persistence;
using Service.Game;

namespace Core.Test.Games.Content;

[TestClass]
public class GetGameTest
{
    private static Mock<IGameService>? MockGameService { get; set; }

    private static Mock<DatabaseContext>? MockDatabase { get; set; }
    
    private static IMapper? Mapper { get; set; }
    
    private static GetGameHandler? GetGameHandler { get; set; }
    
    [ClassInitialize]
    public static void TestClassInit(TestContext context)
    {
        MockGameService = new Mock<IGameService>();
     
        MockDatabase = new Mock<DatabaseContext>();

        var mappingConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile<MappingProfiles>();
        });
        Mapper = mappingConfig.CreateMapper();

        GetGameHandler = new GetGameHandler(MockDatabase.Object, MockGameService.Object, Mapper);
    }

    [TestCleanup]
    public void TestCaseCleanup()
    {
        MockGameService.Reset();
        MockDatabase.Reset();
    }
    
    [TestMethod]
    public async Task GetGame_CachedFresh()
    {
        // Setup
        long fakeId = 42069;

        var fakeGame = new Game()
        {
            RemoteId = fakeId,
            CoverImageURL = "https://chaoschef.example.com",
            Title = "Chaos Chef",
            Summary = "Won Game of the Year",
            Rating = 100,
            PlatformsString = "PC;PS5;Switch",
            CompaniesString = "Very Indecisive Studios;Overflow",
            LastModifiedOn = DateTime.Now
        };
        
        var query = new GetGameQuery(fakeId);
        
        MockDatabase!.Setup(db => db.Games)
            .ReturnsDbSet(new List<Game> { fakeGame });
        
        // Execute
        var result = await GetGameHandler!.Handle(query, CancellationToken.None);

        // Verify
        MockGameService!.VerifyNoOtherCalls();
        Assert.AreEqual(result.RemoteId, fakeGame.RemoteId);
        Assert.AreEqual(result.CoverImageURL, fakeGame.CoverImageURL);
        Assert.AreEqual(result.Title, fakeGame.Title);
        Assert.AreEqual(result.Summary, fakeGame.Summary);
        Assert.AreEqual(result.Rating, fakeGame.Rating);
        Assert.IsTrue(result.Platforms!.SequenceEqual(fakeGame.PlatformsString.Split(";")));
        Assert.IsTrue(result.Companies!.SequenceEqual(fakeGame.CompaniesString.Split(";")));
    }
    
    [TestMethod]
    public async Task GetGame_CachedOld()
    {
        // Setup
        long fakeId = 42069;

        var fakeGame = new Game()
        {
            RemoteId = fakeId,
            CoverImageURL = "https://chaoschef.example.com",
            Title = "Chaos Chef",
            Summary = "Won Game of the Year",
            Rating = 100,
            PlatformsString = "PC;PS5;Switch",
            CompaniesString = "Very Indecisive Studios;Overflow",
            LastModifiedOn = new DateTime(2022, 1, 1)
        };
        var fakeAPIGame = new APIGame
        (
            fakeId,
            "https://chaoschef.example.com",
            "Chaos Chef",
            "Won Game of the Year",
            100,
            new  () { "PC", "PS5", "Switch" },
            new List<string>() { "Very Indecisive Studios", "Overflow" }
        );
        
        var query = new GetGameQuery(fakeId);
        
        MockDatabase!.Setup(db => db.Games)
            .ReturnsDbSet(new List<Game> { fakeGame });
        MockGameService!.Setup(service => service.GetGameById(fakeId))
            .ReturnsAsync(fakeAPIGame);

        // Execute
        var result = await GetGameHandler!.Handle(query, CancellationToken.None);

        // Verify
        MockGameService.Verify(service => service.GetGameById(fakeId), Times.Once);
        MockDatabase.Verify(database => database.Games.Update(It.Is<Game>(g => g.RemoteId == fakeId)), Times.Once);
        MockDatabase.Verify(database => database.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.AreEqual(result.RemoteId, fakeGame.RemoteId);
        Assert.AreEqual(result.CoverImageURL, fakeGame.CoverImageURL);
        Assert.AreEqual(result.Title, fakeGame.Title);
        Assert.AreEqual(result.Summary, fakeGame.Summary);
        Assert.AreEqual(result.Rating, fakeGame.Rating);
        Assert.IsTrue(result.Platforms!.SequenceEqual(fakeGame.PlatformsString.Split(";")));
        Assert.IsTrue(result.Companies!.SequenceEqual(fakeGame.CompaniesString.Split(";")));
    }
    
    [TestMethod]
    public async Task GetGame_NoCache()
    {
        // Setup
        long fakeId = 42069;
        
        var fakeAPIGame = new APIGame
        (
            fakeId,
            "https://chaoschef.example.com",
            "Chaos Chef",
            "Won Game of the Year",
            100,
            new  () { "PC", "PS5", "Switch" },
            new List<string>() { "Very Indecisive Studios", "Overflow" }
        );
        
        var query = new GetGameQuery(fakeId);
        
        MockDatabase!.Setup(db => db.Games)
            .ReturnsDbSet(new List<Game>());
        MockGameService!.Setup(service => service.GetGameById(fakeId))
            .ReturnsAsync(fakeAPIGame);
        
        // Execute
        var result = await GetGameHandler!.Handle(query, CancellationToken.None);

        // Verify
        MockGameService.Verify(service => service.GetGameById(fakeId), Times.Once);
        MockDatabase.Verify(database => database.Games.Add(It.Is<Game>(g => g.RemoteId == fakeId)), Times.Once);
        MockDatabase.Verify(database => database.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.AreEqual(result.RemoteId, fakeAPIGame.Id);
        Assert.AreEqual(result.CoverImageURL, fakeAPIGame.CoverImageURL);
        Assert.AreEqual(result.Title, fakeAPIGame.Title);
        Assert.AreEqual(result.Summary, fakeAPIGame.Summary);
        Assert.AreEqual(result.Rating, fakeAPIGame.Rating);
        Assert.IsTrue(result.Platforms!.SequenceEqual(fakeAPIGame.Platforms));
        Assert.IsTrue(result.Companies!.SequenceEqual(fakeAPIGame.Companies));
    }
    
    [TestMethod]
    public async Task GetGame_NotFound()
    {
        // Setup
        long fakeId = 42069;
        
        var query = new GetGameQuery(fakeId);
        
        MockDatabase!.Setup(db => db.Games)
            .ReturnsDbSet(new List<Game>());
        MockGameService!.Setup(service => service.GetGameById(fakeId))
            .ReturnsAsync((APIGame?) null);
        
        // Execute
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => GetGameHandler!.Handle(query, CancellationToken.None));

        // Verify
        MockGameService.Verify(service => service.GetGameById(fakeId), Times.Once);
    }
}