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
public class GetTrackedGamesTest
{
    private static SqliteConnection? Connection { get; set; }
    
    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }
    
    private static DatabaseContext? InMemDatabase { get; set; }

    private static IMapper? Mapper { get; set; }

    private static GetTrackedGamesHandler? GetTrackedGamesHandler { get; set; }

    private const string FakeUserRemoteId = "d33Z_NuT5";
    private const string FakeDiffUserRemoteId = "d33Z_NuT5+L+M41d3Nl35S";

    [ClassInitialize]
    public static async Task TestClassInit(TestContext context)
    {
        var fakeTrackedGamesList = new List<TrackedGame>()
        {
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                GameRemoteId = 0,
                HoursPlayed = 100,
                Platform = "PSP",
                Format = TrackedGameFormat.Digital,
                Status = TrackedGameStatus.Playing,
                Ownership = TrackedGameOwnership.Wishlist
            },
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                GameRemoteId = 1,
                HoursPlayed = 90,
                Platform = "PC",
                Format = TrackedGameFormat.Digital,
                Status = TrackedGameStatus.Playing,
                Ownership = TrackedGameOwnership.Subscription
            },
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                GameRemoteId = 2,
                HoursPlayed = 80,
                Platform = "XONE",
                Format = TrackedGameFormat.Physical,
                Status = TrackedGameStatus.Paused,
                Ownership = TrackedGameOwnership.Owned
            },
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                GameRemoteId = 3,
                HoursPlayed = 70,
                Platform = "Switch",
                Format = TrackedGameFormat.Physical,
                Status = TrackedGameStatus.Planning,
                Ownership = TrackedGameOwnership.Loan
            },
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                GameRemoteId = 4,
                HoursPlayed = 60,
                Platform = "PC",
                Format = TrackedGameFormat.Digital,
                Status = TrackedGameStatus.Planning,
                Ownership = TrackedGameOwnership.Owned
            },
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                GameRemoteId = 5,
                HoursPlayed = 50,
                Platform = "PS4",
                Format = TrackedGameFormat.Physical,
                Status = TrackedGameStatus.Completed,
                Ownership = TrackedGameOwnership.Loan
            },
            new()
            {
                UserRemoteId = FakeDiffUserRemoteId,
                GameRemoteId = 6,
                HoursPlayed = 25,
                Platform = "PC",
                Format = TrackedGameFormat.Physical,
                Status = TrackedGameStatus.Paused,
                Ownership = TrackedGameOwnership.Loan
            }
        };

        // Setup in memory database
        Connection = new SqliteConnection("Filename=:memory:");
        Connection.Open();

        ContextOptions = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite(Connection)
            .Options;

        InMemDatabase = new DatabaseContext(ContextOptions);
        await InMemDatabase.Database.EnsureCreatedAsync();
        InMemDatabase.TrackedGames.AddRange(fakeTrackedGamesList);
        await InMemDatabase.SaveChangesAsync();

        var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile<MappingProfiles>(); });
        Mapper = mappingConfig.CreateMapper();

        GetTrackedGamesHandler = new GetTrackedGamesHandler(InMemDatabase, Mapper);
    }

    [ClassCleanup]
    public static async Task TestClassCleanup()
    {
        await Connection!.DisposeAsync();
    }


    [TestMethod]
    public async Task GetTrackedGames_Default()
    {
        // Setup
        var query = new GetTrackedGamesQuery(FakeUserRemoteId);

        // Execute
        var result = await GetTrackedGamesHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual(result.TotalCount, 6);
    }

    [TestMethod]
    public async Task GetTrackedGames_ByGameStatus()
    {
        // Setup
        var queryCompleted = new GetTrackedGamesQuery(FakeUserRemoteId)
        {
            GameStatus = TrackedGameStatus.Completed,
        };
        var queryPlaying = new GetTrackedGamesQuery(FakeUserRemoteId)
        {
            GameStatus = TrackedGameStatus.Playing,
        };
        var queryPaused = new GetTrackedGamesQuery(FakeUserRemoteId)
        {
            GameStatus = TrackedGameStatus.Paused,
        };
        var queryPlanning = new GetTrackedGamesQuery(FakeUserRemoteId)
        {
            GameStatus = TrackedGameStatus.Planning,
        };

        // Execute
        var resultCompleted = await GetTrackedGamesHandler!.Handle(queryCompleted, CancellationToken.None);
        var resultPlaying = await GetTrackedGamesHandler.Handle(queryPlaying, CancellationToken.None);
        var resultPaused = await GetTrackedGamesHandler.Handle(queryPaused, CancellationToken.None);
        var resultPlanning = await GetTrackedGamesHandler.Handle(queryPlanning, CancellationToken.None);

        // Verify
        Assert.AreEqual(resultCompleted.TotalCount, 1);
        Assert.AreEqual(resultPlaying.TotalCount, 2);
        Assert.AreEqual(resultPaused.TotalCount, 1);
        Assert.AreEqual(resultPlanning.TotalCount, 2);
    }

    [TestMethod]
    public async Task GetTrackedGames_SortByHoursPlayed()
    {
        // Setup
        var query = new GetTrackedGamesQuery(FakeUserRemoteId)
        {
            SortByHoursPlayed = true
        };

        // Execute
        var result = await GetTrackedGamesHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual(result.TotalCount, 6);
        Assert.AreEqual(result.Items.First().HoursPlayed, 50);
        Assert.AreEqual(result.Items.Last().HoursPlayed, 100);
    }

    [TestMethod]
    public async Task GetTrackedGames_SortByPlatform()
    {
        // Setup
        var query = new GetTrackedGamesQuery(FakeUserRemoteId)
        {
            SortByPlatform = true
        };

        // Execute
        var result = await GetTrackedGamesHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual(result.TotalCount, 6);
        Assert.AreEqual(result.Items.First().Platform, "PC");
        Assert.AreEqual(result.Items.Last().Platform, "XONE");
    }

    [TestMethod]
    public async Task GetTrackedGames_SortByFormat()
    {
        // Setup
        var query = new GetTrackedGamesQuery(FakeUserRemoteId)
        {
            SortByFormat = true
        };

        // Execute
        var result = await GetTrackedGamesHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual(result.TotalCount, 6);
        Assert.AreEqual(result.Items.First().Format, TrackedGameFormat.Digital);
        Assert.AreEqual(result.Items.Last().Format, TrackedGameFormat.Physical);
    }

    [TestMethod]
    public async Task GetTrackedGames_SortByOwnership()
    {
        // Setup
        var query = new GetTrackedGamesQuery(FakeUserRemoteId)
        {
            SortByOwnership = true
        };

        // Execute
        var result = await GetTrackedGamesHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual(result.TotalCount, 6);
        Assert.AreEqual(result.Items.First().Ownership, TrackedGameOwnership.Owned);
        Assert.AreEqual(result.Items.Last().Ownership, TrackedGameOwnership.Subscription);
    }
}