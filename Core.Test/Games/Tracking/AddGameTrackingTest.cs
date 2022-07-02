using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Games.Tracking;
using Core.Exceptions;
using Domain;
using Domain.Media;
using Domain.Tracking;
using Domain.User;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Persistence;
using Service.Game;

namespace Core.Test.Games.Tracking;

[TestClass]
public class AddGameTrackingTest
{
    private static SqliteConnection? Connection { get; set; }

    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }

    private static DatabaseContext? InMemDatabase { get; set; }
    private static Mock<IGameService>? MockGameService { get; set; }

    private static IMapper? Mapper { get; set; }

    private static AddGameTrackingHandler? AddGameTrackingHandler { get; set; }

    private const long FakeExistingGameId = 123;
    private const string FakeExistingUserId = "USEREXIST";

    [ClassInitialize]
    public static async Task TestClassInit(TestContext context)
    {
        var fakeUser = new User()
        {
            RemoteId = FakeExistingUserId
        };

        var fakeGame = new Game()
        {
            RemoteId = FakeExistingGameId
        };

        // Setup in memory database
        Connection = new SqliteConnection("Filename=:memory:");
        Connection.Open();

        ContextOptions = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite(Connection)
            .Options;

        InMemDatabase = new DatabaseContext(ContextOptions);
        await InMemDatabase.Database.EnsureCreatedAsync();

        InMemDatabase.Games.Add(fakeGame);
        InMemDatabase.Users.Add(fakeUser);

        await InMemDatabase.SaveChangesAsync();

        MockGameService = new Mock<IGameService>();

        var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile<MappingProfiles>(); });
        Mapper = mappingConfig.CreateMapper();

        AddGameTrackingHandler = new AddGameTrackingHandler(InMemDatabase, MockGameService.Object, Mapper);
    }

    [ClassCleanup]
    public static async Task TestClassCleanup()
    {
        await Connection!.DisposeAsync();
    }

    [TestCleanup]
    public void TestCaseCleanup()
    {
        MockGameService.Reset();
    }

    [TestMethod]
    public async Task AddGameTracking_Cached()
    {
        // Setup
        var command = new AddGameTrackingCommand(
            FakeExistingUserId,
            FakeExistingGameId,
            200,
            "PC",
            GameTrackingFormat.Digital,
            GameTrackingStatus.Completed,
            GameTrackingOwnership.Owned
        );

        // Execute
        await AddGameTrackingHandler!.Handle(command, CancellationToken.None);

        // Verify
        MockGameService!.Verify(service => service.GetGameById(It.IsAny<long>()), Times.Never);
        var gameTracking = await InMemDatabase!.GameTrackings
            .Where(gt => gt.GameRemoteId.Equals(FakeExistingGameId)
                         && gt.UserRemoteId.Equals(FakeExistingUserId))
            .CountAsync();
        Assert.AreEqual(1, gameTracking);
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
            1,
            new List<string> { "PC" },
            new List<string> { "SONY" }
        );

        var command = new AddGameTrackingCommand(
            FakeExistingUserId,
            fakeAPIGame.Id,
            200,
            "PC",
            GameTrackingFormat.Digital,
            GameTrackingStatus.Completed,
            GameTrackingOwnership.Owned
        );

        MockGameService!.Setup(service => service.GetGameById(fakeAPIGame.Id))
            .ReturnsAsync(fakeAPIGame);

        // Execute
        await AddGameTrackingHandler!.Handle(command, CancellationToken.None);

        // Verify
        MockGameService.Verify(service => service.GetGameById(fakeAPIGame.Id));
        var gameTrackingCount = await InMemDatabase!.GameTrackings
            .Where(gt => gt.GameRemoteId.Equals(fakeAPIGame.Id)
                         && gt.UserRemoteId.Equals(FakeExistingUserId))
            .CountAsync();
        Assert.AreEqual(1, gameTrackingCount);
        var gameCount = await InMemDatabase.Games
            .Where(g => g.RemoteId.Equals(fakeAPIGame.Id))
            .CountAsync();
        Assert.AreEqual(1, gameCount);
    }

    [TestMethod]
    public async Task AddGameTracking_TrackingExists()
    {
        // Setup
        var command = new AddGameTrackingCommand(
            FakeExistingUserId,
            FakeExistingGameId,
            200,
            "PC",
            GameTrackingFormat.Digital,
            GameTrackingStatus.Completed,
            GameTrackingOwnership.Owned
        );

        // Execute & Verify
        await Assert.ThrowsExceptionAsync<ExistsException>(() =>
            AddGameTrackingHandler!.Handle(command, CancellationToken.None));
    }

    [TestMethod]
    public async Task AddGameTracking_GameNotFound()
    {
        // Setup
        var command = new AddGameTrackingCommand(
            FakeExistingUserId,
            999,
            200,
            "PC",
            GameTrackingFormat.Digital,
            GameTrackingStatus.Completed,
            GameTrackingOwnership.Owned
        );

        MockGameService!.Setup(service => service.GetGameById(command.GameRemoteId))
            .ReturnsAsync((APIGame?)null);

        // Execute & Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
            AddGameTrackingHandler!.Handle(command, CancellationToken.None));
        MockGameService.Verify(service => service.GetGameById(It.IsAny<long>()));
    }

    [TestMethod]
    public async Task AddBookTracking_UserNotFound()
    {
        // Setup
        var fakeAPIGame = new APIGame(
            123,
            "",
            "Chaos Chef",
            "Won Game of the Year",
            1,
            new List<string> { "PC" },
            new List<string> { "SONY" }
        );

        var command = new AddGameTrackingCommand(
            "abcd",
            fakeAPIGame.Id,
            200,
            "PC",
            GameTrackingFormat.Digital,
            GameTrackingStatus.Completed,
            GameTrackingOwnership.Owned
        );


        MockGameService!.Setup(service => service.GetGameById(command.GameRemoteId))
            .ReturnsAsync(fakeAPIGame);

        // Execute & Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
            AddGameTrackingHandler!.Handle(command, CancellationToken.None));
    }
}