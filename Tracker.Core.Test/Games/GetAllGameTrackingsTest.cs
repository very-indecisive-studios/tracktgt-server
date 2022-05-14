using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tracker.Core.Games;
using Tracker.Domain;
using Tracker.Persistence;

namespace Tracker.Core.Test.Games;

[TestClass]
public class GetAllGameTrackingsTest
{
    private static SqliteConnection? Connection { get; set; }
    
    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }
    
    private static DatabaseContext? InMemDatabase { get; set; }

    private static IMapper? Mapper { get; set; }

    private static GetAllGameTrackingsHandler? GetAllGameTrackingsHandler { get; set; }

    private const string FakeUserRemoteId = "d33Z_NuT5";
    private const string FakeDiffUserRemoteId = "d33Z_NuT5+L+M41d3Nl35S";

    [ClassInitialize]
    public static async Task TestClassInit(TestContext context)
    {
        var fakeGameTrackingsList = new List<GameTracking>()
        {
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                GameRemoteId = 0,
                HoursPlayed = 100,
                Platform = "PSP",
                Format = GameTrackingFormat.Digital,
                Status = GameTrackingStatus.Playing,
                Ownership = GameTrackingOwnership.Wishlist
            },
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                GameRemoteId = 1,
                HoursPlayed = 90,
                Platform = "PC",
                Format = GameTrackingFormat.Digital,
                Status = GameTrackingStatus.Playing,
                Ownership = GameTrackingOwnership.Subscription
            },
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                GameRemoteId = 2,
                HoursPlayed = 80,
                Platform = "XONE",
                Format = GameTrackingFormat.Physical,
                Status = GameTrackingStatus.Paused,
                Ownership = GameTrackingOwnership.Owned
            },
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                GameRemoteId = 3,
                HoursPlayed = 70,
                Platform = "Switch",
                Format = GameTrackingFormat.Physical,
                Status = GameTrackingStatus.Planning,
                Ownership = GameTrackingOwnership.Loan
            },
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                GameRemoteId = 4,
                HoursPlayed = 60,
                Platform = "PC",
                Format = GameTrackingFormat.Digital,
                Status = GameTrackingStatus.Planning,
                Ownership = GameTrackingOwnership.Owned
            },
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                GameRemoteId = 5,
                HoursPlayed = 50,
                Platform = "PS4",
                Format = GameTrackingFormat.Physical,
                Status = GameTrackingStatus.Completed,
                Ownership = GameTrackingOwnership.Loan
            },
            new()
            {
                UserRemoteId = FakeDiffUserRemoteId,
                GameRemoteId = 6,
                HoursPlayed = 25,
                Platform = "PC",
                Format = GameTrackingFormat.Physical,
                Status = GameTrackingStatus.Paused,
                Ownership = GameTrackingOwnership.Loan
            }
        };

        var fakeGamesList = new List<Game>()
        {
            new() { RemoteId = 0 },
            new() { RemoteId = 1 },
            new() { RemoteId = 2 },
            new() { RemoteId = 3 },
            new() { RemoteId = 4 },
            new() { RemoteId = 5 },
            new() { RemoteId = 6 },
        };
        
        // Setup in memory database
        Connection = new SqliteConnection("Filename=:memory:");
        Connection.Open();

        ContextOptions = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite(Connection)
            .Options;

        InMemDatabase = new DatabaseContext(ContextOptions);
        await InMemDatabase.Database.EnsureCreatedAsync();
        InMemDatabase.GameTrackings.AddRange(fakeGameTrackingsList);
        InMemDatabase.Games.AddRange(fakeGamesList);
        await InMemDatabase.SaveChangesAsync();

        var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile<MappingProfiles>(); });
        Mapper = mappingConfig.CreateMapper();

        GetAllGameTrackingsHandler = new GetAllGameTrackingsHandler(InMemDatabase, Mapper);
    }

    [ClassCleanup]
    public static async Task TestClassCleanup()
    {
        await Connection!.DisposeAsync();
    }


    [TestMethod]
    public async Task GetAllGameTrackings_Default()
    {
        // Setup
        var query = new GetAllGameTrackingsQuery()
        {
            UserRemoteId = FakeUserRemoteId,
        };

        // Execute
        var result = await GetAllGameTrackingsHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual(6, result.TotalCount);
    }

    [TestMethod]
    public async Task GetAllGameTrackings_ByGameStatus()
    {
        // Setup
        var queryCompleted = new GetAllGameTrackingsQuery()
        {
            UserRemoteId = FakeUserRemoteId,
            GameStatus = GameTrackingStatus.Completed,
        };
        var queryPlaying = new GetAllGameTrackingsQuery()
        {
            UserRemoteId = FakeUserRemoteId,
            GameStatus = GameTrackingStatus.Playing,
        };
        var queryPaused = new GetAllGameTrackingsQuery()
        {
            UserRemoteId = FakeUserRemoteId,
            GameStatus = GameTrackingStatus.Paused,
        };
        var queryPlanning = new GetAllGameTrackingsQuery()
        {
            UserRemoteId = FakeUserRemoteId,
            GameStatus = GameTrackingStatus.Planning,
        };

        // Execute
        var resultCompleted = await GetAllGameTrackingsHandler!.Handle(queryCompleted, CancellationToken.None);
        var resultPlaying = await GetAllGameTrackingsHandler.Handle(queryPlaying, CancellationToken.None);
        var resultPaused = await GetAllGameTrackingsHandler.Handle(queryPaused, CancellationToken.None);
        var resultPlanning = await GetAllGameTrackingsHandler.Handle(queryPlanning, CancellationToken.None);

        // Verify
        Assert.AreEqual(1, resultCompleted.TotalCount);
        Assert.AreEqual(2, resultPlaying.TotalCount);
        Assert.AreEqual(1, resultPaused.TotalCount);
        Assert.AreEqual(2, resultPlanning.TotalCount);
    }

    [TestMethod]
    public async Task GetAllGameTrackings_SortByHoursPlayed()
    {
        // Setup
        var query = new GetAllGameTrackingsQuery()
        {
            UserRemoteId = FakeUserRemoteId,
            SortByHoursPlayed = true
        };

        // Execute
        var result = await GetAllGameTrackingsHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual(6, result.TotalCount);
        Assert.AreEqual(50, result.Items.First().HoursPlayed);
        Assert.AreEqual(100, result.Items.Last().HoursPlayed);
    }

    [TestMethod]
    public async Task GetAllGameTrackings_SortByPlatform()
    {
        // Setup
        var query = new GetAllGameTrackingsQuery()
        {
            UserRemoteId = FakeUserRemoteId,
            SortByPlatform = true
        };

        // Execute
        var result = await GetAllGameTrackingsHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual(6, result.TotalCount);
        Assert.AreEqual("PC", result.Items.First().Platform);
        Assert.AreEqual("XONE", result.Items.Last().Platform);
    }

    [TestMethod]
    public async Task GetAllGameTrackings_SortByFormat()
    {
        // Setup
        var query = new GetAllGameTrackingsQuery()
        {
            UserRemoteId = FakeUserRemoteId,
            SortByFormat = true
        };

        // Execute
        var result = await GetAllGameTrackingsHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual(6, result.TotalCount);
        Assert.AreEqual(GameTrackingFormat.Digital, result.Items.First().Format);
        Assert.AreEqual(GameTrackingFormat.Physical, result.Items.Last().Format);
    }

    [TestMethod]
    public async Task GetAllGameTrackings_SortByOwnership()
    {
        // Setup
        var query = new GetAllGameTrackingsQuery()
        {
            UserRemoteId = FakeUserRemoteId,
            SortByOwnership = true
        };

        // Execute
        var result = await GetAllGameTrackingsHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual(6, result.TotalCount);
        Assert.AreEqual(GameTrackingOwnership.Owned, result.Items.First().Ownership);
        Assert.AreEqual(GameTrackingOwnership.Subscription, result.Items.Last().Ownership);
    }
}