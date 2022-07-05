using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Games.Tracking;
using Core.Exceptions;
using Domain.Media;
using Domain.Tracking;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Persistence;

namespace Core.Test.Games.Tracking;

[TestClass]
public class RemoveGameTrackingTest
{
    private static SqliteConnection? Connection { get; set; }

    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }

    private static DatabaseContext? InMemDatabase { get; set; }

    private static IMapper? Mapper { get; set; }

    private static RemoveGameTrackingHandler? RemoveGameTrackingHandler { get; set; }

    private const string FakeUserRemoteId = "d33Z_NuT5";
    private const long FakeGameRemoteId = 0;

    [ClassInitialize]
    public static async Task TestClassInit(TestContext context)
    {
        var fakeGame = new Game()
        {
            RemoteId = FakeGameRemoteId
        };
        
        var fakeGameTrackingsList = new List<GameTracking>()
        {
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                GameRemoteId = FakeGameRemoteId,
                HoursPlayed = 100,
                Platform = "PC",
                Format = GameTrackingFormat.Digital,
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
        InMemDatabase.Games.Add(fakeGame);
        await InMemDatabase.SaveChangesAsync();

        var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile<MappingProfiles>(); });
        Mapper = mappingConfig.CreateMapper();

        RemoveGameTrackingHandler = new RemoveGameTrackingHandler(InMemDatabase);
    }

    [TestMethod]
    public async Task RemoveGameTracking_Exists()
    {
        // Setup
        var command = new RemoveGameTrackingCommand(FakeUserRemoteId, FakeGameRemoteId, "PC");
        
        // Execute
        await RemoveGameTrackingHandler!.Handle(command, CancellationToken.None);

        // Verify
        var count = await InMemDatabase!.GameTrackings
            .Where(b => b.UserRemoteId.Equals(FakeUserRemoteId) 
                        && b.GameRemoteId.Equals(FakeGameRemoteId))
            .CountAsync();
        Assert.AreEqual(0, count);
    }

    [TestMethod]
    public async Task RemoveGameTracking_NotExists()
    {
        // Setup
        var command = new RemoveGameTrackingCommand(FakeUserRemoteId, FakeGameRemoteId, "PS4");

        // Execute
        // Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => 
            RemoveGameTrackingHandler!.Handle(command, CancellationToken.None));
    }
}