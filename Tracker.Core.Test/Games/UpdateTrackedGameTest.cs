using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tracker.Core.Exceptions;
using Tracker.Core.Games;
using Tracker.Domain;
using Tracker.Persistence;

namespace Tracker.Core.Test.Games;

[TestClass]
public class UpdateTrackedGameTest
{
    private static SqliteConnection? Connection { get; set; }

    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }

    private static DatabaseContext? InMemDatabase { get; set; }

    private static UpdateTrackedGameHandler? UpdateTrackedGameHandler { get; set; }

    private static IMapper? Mapper { get; set; }

    [ClassInitialize]
    public static async Task TestClassInit(TestContext context)
    {
        // Setup in memory database
        Connection = new SqliteConnection("Filename=:memory:");
        Connection.Open();

        ContextOptions = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite(Connection)
            .Options;

        InMemDatabase = new DatabaseContext(ContextOptions);
        await InMemDatabase.Database.EnsureCreatedAsync();

        var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile<MappingProfiles>(); });
        Mapper = mappingConfig.CreateMapper();

        UpdateTrackedGameHandler = new UpdateTrackedGameHandler(InMemDatabase, Mapper);
    }

    [ClassCleanup]
    public static async Task TestClassCleanup()
    {
        await Connection!.DisposeAsync();
    }

    [TestMethod]
    public async Task UpdateTrackedGame_Exists()
    {
        // Setup
        var fakeUserRemoteId = "d33Z_NuT5";
        var fakeGameRemoteId = 69;
        var fakeHoursPlayed = 10;
        var fakePlatform = "PC";
        var fakeFormat = GameFormat.Digital;
        var fakeStatus = GameStatus.Planning;
        var fakeOwnership = GameOwnership.Wishlist;
        InMemDatabase!.TrackedGames.Add(new TrackedGame
        {
            UserRemoteId = fakeUserRemoteId,
            GameRemoteId = fakeGameRemoteId,
            HoursPlayed = fakeHoursPlayed,
            Platform = fakePlatform,
            Format = fakeFormat,
            Status = fakeStatus,
            Ownership = fakeOwnership
        });
        await InMemDatabase.SaveChangesAsync(CancellationToken.None);

        var newFakeHoursPlayed = 25;
        var newFakePlatform = "Switch";
        var newFakeFormat = GameFormat.Physical;
        var newFakeStatus = GameStatus.Playing;
        var newFakeOwnership = GameOwnership.Owned;
        var command = new UpdateTrackedGameCommand(fakeUserRemoteId, fakeGameRemoteId, newFakeHoursPlayed,
            newFakePlatform, newFakeFormat, newFakeStatus, newFakeOwnership);
        
        // Execute
         await UpdateTrackedGameHandler!.Handle(command, CancellationToken.None);
        
        // Verify
        var updatedTrackedGame = await InMemDatabase.TrackedGames
            .AsNoTracking()
            .Where(tg => tg.UserRemoteId == fakeUserRemoteId && tg.GameRemoteId == fakeGameRemoteId)
            .FirstOrDefaultAsync(CancellationToken.None);
        Assert.IsNotNull(updatedTrackedGame);
        Assert.AreEqual(updatedTrackedGame.HoursPlayed, newFakeHoursPlayed);
        Assert.AreEqual(updatedTrackedGame.Platform, newFakePlatform);
        Assert.AreEqual(updatedTrackedGame.Format, newFakeFormat);
        Assert.AreEqual(updatedTrackedGame.Status, newFakeStatus);
        Assert.AreEqual(updatedTrackedGame.Ownership, newFakeOwnership);
    }

    [TestMethod]
    public async Task UpdateTrackedGame_NotExists()
    {
        // Setup
        var fakeUserRemoteId = "d33Z_NuT5";
        var fakeDiffUserRemoteId = "d33Z_NuT5_L+M41d3nL3s5";
        var fakeGameRemoteId = 69;
        var fakeDiffGameRemoteId = 420;
        
        var newFakeHoursPlayed = 25;
        var newFakePlatform = "Switch";
        var newFakeFormat = GameFormat.Physical;
        var newFakeStatus = GameStatus.Playing;
        var newFakeOwnership = GameOwnership.Owned;
        
        var commandDiffUser = new UpdateTrackedGameCommand(fakeDiffUserRemoteId, fakeGameRemoteId, newFakeHoursPlayed,
            newFakePlatform, newFakeFormat, newFakeStatus, newFakeOwnership);
        var commandDiffGame = new UpdateTrackedGameCommand(fakeUserRemoteId, fakeDiffGameRemoteId, newFakeHoursPlayed,
            newFakePlatform, newFakeFormat, newFakeStatus, newFakeOwnership);
        var commandDiffUserAndUser = new UpdateTrackedGameCommand(fakeDiffUserRemoteId, fakeDiffGameRemoteId, newFakeHoursPlayed,
            newFakePlatform, newFakeFormat, newFakeStatus, newFakeOwnership);
        
        // Execute
        // Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
            UpdateTrackedGameHandler!.Handle(commandDiffUser, CancellationToken.None));
        await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
            UpdateTrackedGameHandler!.Handle(commandDiffGame, CancellationToken.None));
        await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
            UpdateTrackedGameHandler!.Handle(commandDiffUserAndUser, CancellationToken.None));
    }
}