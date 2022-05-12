using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.EntityFrameworkCore;
using Tracker.Core.Exceptions;
using Tracker.Core.Games;
using Tracker.Domain;
using Tracker.Persistence;
using Tracker.Service.Game;

namespace Tracker.Core.Test.Games;

[TestClass]
public class AddTrackedGameTest
{
    private static Mock<IGameService>? MockGameService { get; set; }

    private static Mock<DatabaseContext>? MockDatabase { get; set; }
    
    private static IMapper? Mapper { get; set; }
    
    private static AddTrackedGameHandler? AddTrackedGameHandler { get; set; }
    
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

        AddTrackedGameHandler = new AddTrackedGameHandler(MockDatabase.Object, MockGameService.Object, Mapper);
    }

    [TestCleanup]
    public void TestCaseCleanup()
    {
        MockGameService.Reset();
        MockDatabase.Reset();
    }

    [TestMethod]
    public async Task AddTrackedGame_Cached()
    {
        // Setup
        var fakeGame = new Game()
        {
            RemoteId = 42069,
            Title = "Chaos Chef"
        };
        
        var command = new AddTrackedGameCommand(
            "abcd",
            fakeGame.RemoteId,
            200,
            "PC",
            TrackedGameFormat.Digital,
            TrackedGameStatus.Planning,
            TrackedGameOwnership.Owned
        );
        
        MockDatabase!.Setup(db => db.Games)
            .ReturnsDbSet(new List<Game> { fakeGame });        
        MockDatabase.Setup(db => db.TrackedGames)
            .ReturnsDbSet(new List<TrackedGame>());
        MockDatabase.Setup(db => db.Users)
            .ReturnsDbSet(new List<User>() { new() { RemoteId = "abcd"} });

        // Execute
        await AddTrackedGameHandler!.Handle(command, CancellationToken.None);
        
        // Verify
        MockGameService!.Verify(service => service.GetGameById(It.IsAny<long>()), Times.Never);
        MockDatabase.Verify(database => database.TrackedGames.Add(It.IsAny<TrackedGame>()));
        MockDatabase.Verify(database => database.Games.Add(It.IsAny<Game>()), Times.Never);
        MockDatabase.Verify(database => database.SaveChangesAsync(CancellationToken.None));
    }
    
    [TestMethod]
    public async Task AddTrackedGame_NoCached_APIHit()
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

        var command = new AddTrackedGameCommand(
            "abcd",
            fakeAPIGame.Id,
            200,
            "PC",
            TrackedGameFormat.Digital,
            TrackedGameStatus.Planning,
            TrackedGameOwnership.Owned
        );
        
        MockDatabase!.Setup(db => db.Games)
            .ReturnsDbSet(new List<Game>());        
        MockDatabase.Setup(db => db.TrackedGames)
            .ReturnsDbSet(new List<TrackedGame>());
        MockDatabase.Setup(db => db.Users)
            .ReturnsDbSet(new List<User>() { new() { RemoteId = "abcd"} });
        
        MockGameService!.Setup(service => service.GetGameById(command.GameRemoteId))
            .ReturnsAsync(fakeAPIGame);
        
        // Execute
        await AddTrackedGameHandler!.Handle(command, CancellationToken.None);
        
        // Verify
        MockGameService.Verify(service => service.GetGameById(It.IsAny<long>()));
        MockDatabase.Verify(database => database.TrackedGames.Add(It.IsAny<TrackedGame>()));
        MockDatabase.Verify(database => database.Games.Add(It.IsAny<Game>()));
        MockDatabase.Verify(database => database.SaveChangesAsync(CancellationToken.None));
    }
    
    [TestMethod]
    public async Task AddTrackedGame_GameNotFound()
    {
        // Setup
        var command = new AddTrackedGameCommand(
            "abcd",
            42069,
            200,
            "PC",
            TrackedGameFormat.Digital,
            TrackedGameStatus.Planning,
            TrackedGameOwnership.Owned
        );
        
        MockDatabase!.Setup(db => db.Games)
            .ReturnsDbSet(new List<Game>());        
        MockDatabase.Setup(db => db.TrackedGames)
            .ReturnsDbSet(new List<TrackedGame>());
        MockDatabase.Setup(db => db.Users)
            .ReturnsDbSet(new List<User>() { new() { RemoteId = "abcd"} });
        
        MockGameService!.Setup(service => service.GetGameById(command.GameRemoteId))
            .ReturnsAsync((APIGame?) null);
        
        // Execute & Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => AddTrackedGameHandler!.Handle(command, CancellationToken.None));
        MockGameService.Verify(service => service.GetGameById(It.IsAny<long>()));
        MockDatabase.Verify(database => database.TrackedGames.Add(It.IsAny<TrackedGame>()), Times.Never);
        MockDatabase.Verify(database => database.Games.Add(It.IsAny<Game>()), Times.Never);
        MockDatabase.Verify(database => database.SaveChangesAsync(CancellationToken.None), Times.Never);
    }
    
    [TestMethod]
    public async Task AddTrackedGame_UserNotFound()
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


        var command = new AddTrackedGameCommand(
            "abcd",
            fakeAPIGame.Id,
            200,
            "PC",
            TrackedGameFormat.Digital,
            TrackedGameStatus.Planning,
            TrackedGameOwnership.Owned
        );

        MockDatabase!.Setup(db => db.Games)
            .ReturnsDbSet(new List<Game>());        
        MockDatabase.Setup(db => db.TrackedGames)
            .ReturnsDbSet(new List<TrackedGame>());
        MockDatabase.Setup(db => db.Users)
            .ReturnsDbSet(new List<User>());
        
        MockGameService!.Setup(service => service.GetGameById(command.GameRemoteId))
            .ReturnsAsync(fakeAPIGame);
        
        // Execute & Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => AddTrackedGameHandler!.Handle(command, CancellationToken.None));
        MockGameService.Verify(service => service.GetGameById(It.IsAny<long>()), Times.Never);
        MockDatabase.Verify(database => database.TrackedGames.Add(It.IsAny<TrackedGame>()), Times.Never);
        MockDatabase.Verify(database => database.Games.Add(It.IsAny<Game>()), Times.Never);
        MockDatabase.Verify(database => database.SaveChangesAsync(CancellationToken.None), Times.Never);
    }
}