using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.EntityFrameworkCore;
using Tracker.Core;
using Tracker.Core.Exceptions;
using Tracker.Core.Games;
using Tracker.Domain;
using Tracker.Persistence;
using Tracker.Service.Game;

namespace Tracker.Core.Test.Games;

[TestClass]
public class AddTrackedGameTest
{
    private Mock<IGameService> _mockGameService { get; set; }

    private Mock<DatabaseContext> _mockDatabase { get; set; }
    
    private IMapper _mapper { get; set; }
    
    private AddTrackedGameHandler AddTrackedGameHandler { get; set; }
    
    [TestInitialize]
    public void TestClassInit()
    {
        _mockGameService = new Mock<IGameService>();
     
        _mockDatabase = new Mock<DatabaseContext>();

        var mappingConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile<MappingProfiles>();
        });
        _mapper = mappingConfig.CreateMapper();

        AddTrackedGameHandler = new AddTrackedGameHandler(_mockDatabase.Object, _mockGameService.Object, _mapper);
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
        
        var command = new AddTrackedGameCommand
        {
            UserId = default,
            GameId = fakeGame.RemoteId,
            HoursPlayed = 200,
            Platform = "PC",
            Format = GameFormat.Digital,
            Status = GameStatus.Current,
            Ownership = GameOwnership.Owned
        };
        
        _mockDatabase.Setup(db => db.Games)
            .ReturnsDbSet(new List<Game> { fakeGame });        
        _mockDatabase.Setup(db => db.TrackedGames)
            .ReturnsDbSet(new List<TrackedGame>());

        // Execute
        await AddTrackedGameHandler.Handle(command, CancellationToken.None);
        
        // Verify
        _mockGameService.Verify(service => service.GetGameById(It.IsAny<long>()), Times.Never);
        _mockDatabase.Verify(database => database.TrackedGames.Add(It.IsAny<TrackedGame>()));
        _mockDatabase.Verify(database => database.Games.Add(It.IsAny<Game>()), Times.Never);
        _mockDatabase.Verify(database => database.SaveChangesAsync(CancellationToken.None));
    }
    
    [TestMethod]
    public async Task AddTrackedGame_NoCached_APIHit()
    {
        // Setup
        var fakeAPIGame = new APIGame()
        {
            Id = 42069,
            Title = "Chaos Chef",
            Platforms = new List<string> { "PC" }
        };
        
        var command = new AddTrackedGameCommand
        {
            UserId = default,
            GameId = fakeAPIGame.Id,
            HoursPlayed = 200,
            Platform = "PC",
            Format = GameFormat.Digital,
            Status = GameStatus.Current,
            Ownership = GameOwnership.Owned
        };
        
        _mockDatabase.Setup(db => db.Games)
            .ReturnsDbSet(new List<Game>());        
        _mockDatabase.Setup(db => db.TrackedGames)
            .ReturnsDbSet(new List<TrackedGame>());
        
        _mockGameService.Setup(service => service.GetGameById(command.GameId))
            .ReturnsAsync(fakeAPIGame);
        
        // Execute
        await AddTrackedGameHandler.Handle(command, CancellationToken.None);
        
        // Verify
        _mockGameService.Verify(service => service.GetGameById(It.IsAny<long>()));
        _mockDatabase.Verify(database => database.TrackedGames.Add(It.IsAny<TrackedGame>()));
        _mockDatabase.Verify(database => database.Games.Add(It.IsAny<Game>()));
        _mockDatabase.Verify(database => database.SaveChangesAsync(CancellationToken.None));
    }
    
    [TestMethod]
    public async Task AddTrackedGame_GameNotFound()
    {
        // Setup
        var command = new AddTrackedGameCommand
        {
            UserId = default,
            GameId = 42069,
            HoursPlayed = 200,
            Platform = "PC",
            Format = GameFormat.Digital,
            Status = GameStatus.Current,
            Ownership = GameOwnership.Owned
        };
        
        _mockDatabase.Setup(db => db.Games)
            .ReturnsDbSet(new List<Game>());        
        _mockDatabase.Setup(db => db.TrackedGames)
            .ReturnsDbSet(new List<TrackedGame>());
        
        _mockGameService.Setup(service => service.GetGameById(command.GameId))
            .ReturnsAsync((APIGame?) null);
        
        // Execute & Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => AddTrackedGameHandler.Handle(command, CancellationToken.None));
        _mockGameService.Verify(service => service.GetGameById(It.IsAny<long>()));
        _mockDatabase.Verify(database => database.TrackedGames.Add(It.IsAny<TrackedGame>()), Times.Never);
        _mockDatabase.Verify(database => database.Games.Add(It.IsAny<Game>()), Times.Never);
        _mockDatabase.Verify(database => database.SaveChangesAsync(CancellationToken.None), Times.Never);
    }
    
    [TestMethod]
    public void AddTrackedGame_UserNotFound()
    {
    }
}