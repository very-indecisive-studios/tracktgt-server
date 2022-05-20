using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.EntityFrameworkCore;
using Core.Exceptions;
using Core.Games;
using Domain;
using Persistence;
using Service.Game;

namespace Core.Test.Games;

[TestClass]
public class AddGameTrackingTest
{
    private static Mock<IGameService>? MockGameService { get; set; }

    private static Mock<DatabaseContext>? MockDatabase { get; set; }
    
    private static IMapper? Mapper { get; set; }
    
    private static AddGameTrackingHandler? AddGameTrackingHandler { get; set; }
    
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

        AddGameTrackingHandler = new AddGameTrackingHandler(MockDatabase.Object, MockGameService.Object, Mapper);
    }

    [TestCleanup]
    public void TestCaseCleanup()
    {
        MockGameService.Reset();
        MockDatabase.Reset();
    }

    [TestMethod]
    public async Task AddGameTracking_Cached()
    {
        // Setup
        var fakeGame = new Game()
        {
            RemoteId = 42069,
            Title = "Chaos Chef"
        };
        
        var command = new AddGameTrackingCommand(
            "abcd",
            fakeGame.RemoteId,
            200,
            "PC",
            GameTrackingFormat.Digital,
            GameTrackingStatus.Planning,
            GameTrackingOwnership.Owned
        );
        
        MockDatabase!.Setup(db => db.Games)
            .ReturnsDbSet(new List<Game> { fakeGame });        
        MockDatabase.Setup(db => db.GameTrackings)
            .ReturnsDbSet(new List<GameTracking>());
        MockDatabase.Setup(db => db.Users)
            .ReturnsDbSet(new List<User>() { new() { RemoteId = "abcd"} });

        // Execute
        await AddGameTrackingHandler!.Handle(command, CancellationToken.None);
        
        // Verify
        MockGameService!.Verify(service => service.GetGameById(It.IsAny<long>()), Times.Never);
        MockDatabase.Verify(database => database.GameTrackings.Add(It.IsAny<GameTracking>()));
        MockDatabase.Verify(database => database.Games.Add(It.IsAny<Game>()), Times.Never);
        MockDatabase.Verify(database => database.SaveChangesAsync(CancellationToken.None));
    }
    
    [TestMethod]
    public async Task AddGameTracking_NoCached_APIHit()
    {
        // Setup
        var fakeAPIGame = new APIGame(
            42069,
            "",
            "Chaos Chef",
            "Won Game of the Year",
            100,
            new List<string> { "PC" },
            new List<string> { "Very Indecisive Studios" }
        );

        var command = new AddGameTrackingCommand(
            "abcd",
            fakeAPIGame.Id,
            200,
            "PC",
            GameTrackingFormat.Digital,
            GameTrackingStatus.Planning,
            GameTrackingOwnership.Owned
        );
        
        MockDatabase!.Setup(db => db.Games)
            .ReturnsDbSet(new List<Game>());        
        MockDatabase.Setup(db => db.GameTrackings)
            .ReturnsDbSet(new List<GameTracking>());
        MockDatabase.Setup(db => db.Users)
            .ReturnsDbSet(new List<User>() { new() { RemoteId = "abcd"} });
        
        MockGameService!.Setup(service => service.GetGameById(command.GameRemoteId))
            .ReturnsAsync(fakeAPIGame);
        
        // Execute
        await AddGameTrackingHandler!.Handle(command, CancellationToken.None);
        
        // Verify
        MockGameService.Verify(service => service.GetGameById(It.IsAny<long>()));
        MockDatabase.Verify(database => database.GameTrackings.Add(It.IsAny<GameTracking>()));
        MockDatabase.Verify(database => database.Games.Add(It.IsAny<Game>()));
        MockDatabase.Verify(database => database.SaveChangesAsync(CancellationToken.None));
    }
    
    [TestMethod]
    public async Task AddGameTracking_GameNotFound()
    {
        // Setup
        var command = new AddGameTrackingCommand(
            "abcd",
            42069,
            200,
            "PC",
            GameTrackingFormat.Digital,
            GameTrackingStatus.Planning,
            GameTrackingOwnership.Owned
        );
        
        MockDatabase!.Setup(db => db.Games)
            .ReturnsDbSet(new List<Game>());        
        MockDatabase.Setup(db => db.GameTrackings)
            .ReturnsDbSet(new List<GameTracking>());
        MockDatabase.Setup(db => db.Users)
            .ReturnsDbSet(new List<User>() { new() { RemoteId = "abcd"} });
        
        MockGameService!.Setup(service => service.GetGameById(command.GameRemoteId))
            .ReturnsAsync((APIGame?) null);
        
        // Execute & Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => AddGameTrackingHandler!.Handle(command, CancellationToken.None));
        MockGameService.Verify(service => service.GetGameById(It.IsAny<long>()));
        MockDatabase.Verify(database => database.GameTrackings.Add(It.IsAny<GameTracking>()), Times.Never);
        MockDatabase.Verify(database => database.Games.Add(It.IsAny<Game>()), Times.Never);
        MockDatabase.Verify(database => database.SaveChangesAsync(CancellationToken.None), Times.Never);
    }
    
    [TestMethod]
    public async Task AddGameTracking_UserNotFound()
    {
        // Setup
        var fakeAPIGame = new APIGame(
            42069,
            "",
            "Chaos Chef",
            "Won Game of the Year",
            100,
            new List<string> { "PC" },
            new List<string> { "Very Indecisive Studios" }
        );


        var command = new AddGameTrackingCommand(
            "abcd",
            fakeAPIGame.Id,
            200,
            "PC",
            GameTrackingFormat.Digital,
            GameTrackingStatus.Planning,
            GameTrackingOwnership.Owned
        );

        MockDatabase!.Setup(db => db.Games)
            .ReturnsDbSet(new List<Game>());        
        MockDatabase.Setup(db => db.GameTrackings)
            .ReturnsDbSet(new List<GameTracking>());
        MockDatabase.Setup(db => db.Users)
            .ReturnsDbSet(new List<User>());
        
        MockGameService!.Setup(service => service.GetGameById(command.GameRemoteId))
            .ReturnsAsync(fakeAPIGame);
        
        // Execute & Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => AddGameTrackingHandler!.Handle(command, CancellationToken.None));
        MockGameService.Verify(service => service.GetGameById(It.IsAny<long>()), Times.Never);
        MockDatabase.Verify(database => database.GameTrackings.Add(It.IsAny<GameTracking>()), Times.Never);
        MockDatabase.Verify(database => database.Games.Add(It.IsAny<Game>()), Times.Never);
        MockDatabase.Verify(database => database.SaveChangesAsync(CancellationToken.None), Times.Never);
    }
}