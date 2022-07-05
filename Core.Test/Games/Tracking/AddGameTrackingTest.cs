using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Games.Tracking;
using Core.Exceptions;
using Domain.Media;
using Domain.Tracking;
using Domain.User;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Persistence;
using Service.Game;

namespace Core.Test.Games.Tracking;

[TestClass]
public class AddGameTrackingTest
{
    private static SqliteConnection? Connection { get; set; }

    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }

    private static DatabaseContext? InMemDatabase { get; set; }

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

        var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile<MappingProfiles>(); });
        Mapper = mappingConfig.CreateMapper();

        AddGameTrackingHandler = new AddGameTrackingHandler(InMemDatabase, Mapper);
    }

    [ClassCleanup]
    public static async Task TestClassCleanup()
    {
        await Connection!.DisposeAsync();
    }

    [TestMethod]
    public async Task AddGameTracking_Default()
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
        var gameTracking = await InMemDatabase!.GameTrackings
            .Where(gt => gt.GameRemoteId.Equals(FakeExistingGameId)
                         && gt.UserRemoteId.Equals(FakeExistingUserId))
            .CountAsync();
        Assert.AreEqual(1, gameTracking);
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
        
        // Execute & Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
            AddGameTrackingHandler!.Handle(command, CancellationToken.None));
    }

    [TestMethod]
    public async Task AddBookTracking_UserNotFound()
    {
        var command = new AddGameTrackingCommand(
            "abcd",
            123,
            200,
            "PC",
            GameTrackingFormat.Digital,
            GameTrackingStatus.Completed,
            GameTrackingOwnership.Owned
        );

        // Execute & Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
            AddGameTrackingHandler!.Handle(command, CancellationToken.None));
    }
}