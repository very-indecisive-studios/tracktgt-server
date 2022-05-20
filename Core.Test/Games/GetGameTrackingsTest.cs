using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Games;
using Domain;
using Persistence;

namespace Core.Test.Games;

[TestClass]
public class GetGameTrackingsTest
{
    private static SqliteConnection? Connection { get; set; }

    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }

    private static DatabaseContext? InMemDatabase { get; set; }

    private static IMapper? Mapper { get; set; }

    private static GetGameTrackingsHandler? GetGameTrackingsHandler { get; set; }

    private const string FakeUserRemoteId = "d33Z_NuT5";
    private const long FakeGameRemoteId = 0;

    [ClassInitialize]
    public static async Task TestClassInit(TestContext context)
    {
        var fakeGameTrackingsList = new List<GameTracking>()
        {
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                GameRemoteId = FakeGameRemoteId,
                HoursPlayed = 100,
                Platform = "PSP",
                Format = GameTrackingFormat.Digital,
                Status = GameTrackingStatus.Playing,
                Ownership = GameTrackingOwnership.Wishlist
            },
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                GameRemoteId = FakeGameRemoteId,
                HoursPlayed = 90,
                Platform = "PC",
                Format = GameTrackingFormat.Digital,
                Status = GameTrackingStatus.Playing,
                Ownership = GameTrackingOwnership.Subscription
            },
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                GameRemoteId = 42069,
                HoursPlayed = 80,
                Platform = "XONE",
                Format = GameTrackingFormat.Physical,
                Status = GameTrackingStatus.Paused,
                Ownership = GameTrackingOwnership.Owned
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
        InMemDatabase.GameTrackings.AddRange(fakeGameTrackingsList);
        await InMemDatabase.SaveChangesAsync();

        var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile<MappingProfiles>(); });
        Mapper = mappingConfig.CreateMapper();

        GetGameTrackingsHandler = new GetGameTrackingsHandler(InMemDatabase, Mapper);
    }

    [ClassCleanup]
    public static async Task TestClassCleanup()
    {
        await Connection!.DisposeAsync();
    }

    [TestMethod]
    public async Task GetGameTrackings_SameGameDiffPlatforms()
    {
        // Setup
        var query = new GetGameTrackingsQuery(FakeUserRemoteId, FakeGameRemoteId);

        // Execute
        var result = await GetGameTrackingsHandler!.Handle(query, CancellationToken.None);
        
        // Verify
        Assert.AreEqual(2, result.Items.Count);
    }
}