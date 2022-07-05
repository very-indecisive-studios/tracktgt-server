using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Exceptions;
using Core.Games.Tracking;
using Domain;
using Domain.Media;
using Domain.Tracking;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Persistence;

namespace Core.Test.Games.Tracking;

[TestClass]
public class UpdateGameTrackingTest
{
    private static SqliteConnection? Connection { get; set; }

    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }

    private static DatabaseContext? InMemDatabase { get; set; }

    private static UpdateGameTrackingHandler? UpdateGameTrackingHandler { get; set; }

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

        UpdateGameTrackingHandler = new UpdateGameTrackingHandler(InMemDatabase, Mapper);
    }

    [ClassCleanup]
    public static async Task TestClassCleanup()
    {
        await Connection!.DisposeAsync();
    }

    [TestMethod]
    public async Task UpdateGameTracking_Exists()
    {
        // Setup
        var fakeUserRemoteId = "d33Z_NuT5";
        var fakeGameRemoteId = 69;
        var fakeHoursPlayed = 10;
        var fakePlatform = "PC";
        var fakeFormat = GameTrackingFormat.Digital;
        var fakeStatus = GameTrackingStatus.Planning;
        var fakeOwnership = GameTrackingOwnership.Subscription;
        InMemDatabase!.GameTrackings.Add(new GameTracking
        {
            UserRemoteId = fakeUserRemoteId,
            GameRemoteId = fakeGameRemoteId,
            HoursPlayed = fakeHoursPlayed,
            Platform = fakePlatform,
            Format = fakeFormat,
            Status = fakeStatus,
            Ownership = fakeOwnership
        });
        InMemDatabase.Games.Add(new Game
        {
            RemoteId = fakeGameRemoteId
        });
        await InMemDatabase.SaveChangesAsync(CancellationToken.None);

        var newFakeHoursPlayed = 25;
        var newFakeFormat = GameTrackingFormat.Physical;
        var newFakeStatus = GameTrackingStatus.Playing;
        var newFakeOwnership = GameTrackingOwnership.Owned;
        var command = new UpdateGameTrackingCommand(fakeUserRemoteId, fakeGameRemoteId, fakePlatform, newFakeHoursPlayed, 
            newFakeFormat, newFakeStatus, newFakeOwnership);
        
        // Execute
         await UpdateGameTrackingHandler!.Handle(command, CancellationToken.None);
        
        // Verify
        var updatedGameTracking = await InMemDatabase.GameTrackings
            .AsNoTracking()
            .Where(tg => tg.UserRemoteId == fakeUserRemoteId && tg.GameRemoteId == fakeGameRemoteId)
            .FirstOrDefaultAsync(CancellationToken.None);
        Assert.IsNotNull(updatedGameTracking);
        Assert.AreEqual(updatedGameTracking.HoursPlayed, newFakeHoursPlayed);
        Assert.AreEqual(updatedGameTracking.Platform, fakePlatform);
        Assert.AreEqual(updatedGameTracking.Format, newFakeFormat);
        Assert.AreEqual(updatedGameTracking.Status, newFakeStatus);
        Assert.AreEqual(updatedGameTracking.Ownership, newFakeOwnership);
        
        var activity = await InMemDatabase.Activities
            .Where(a => a.UserRemoteId.Equals(fakeUserRemoteId))
            .FirstOrDefaultAsync();
        Assert.IsNotNull(activity);
        Assert.AreEqual(ActivityMediaType.Game, activity.MediaType);
        Assert.AreEqual(ActivityAction.Update, activity.Action);
    }

    [TestMethod]
    public async Task UpdateGameTracking_NotExists()
    {
        // Setup
        var fakeUserRemoteId = "d33Z_NuT5";
        var fakeDiffUserRemoteId = "d33Z_NuT5_L+M41d3nL3s5";
        var fakeGameRemoteId = 69;
        var fakeDiffGameRemoteId = 420;
        var fakePlatform = "PC";
        var fakeDiffPlatform = "Switch";
        
        var newFakeHoursPlayed = 25;
        var newFakeFormat = GameTrackingFormat.Physical;
        var newFakeStatus = GameTrackingStatus.Playing;
        var newFakeOwnership = GameTrackingOwnership.Owned;
        
        var commandDiffUser = new UpdateGameTrackingCommand(fakeDiffUserRemoteId, fakeGameRemoteId, fakePlatform,
            newFakeHoursPlayed, newFakeFormat, newFakeStatus, newFakeOwnership);
        var commandDiffGame = new UpdateGameTrackingCommand(fakeUserRemoteId, fakeDiffGameRemoteId, fakePlatform,
            newFakeHoursPlayed, newFakeFormat, newFakeStatus, newFakeOwnership);
        var commandDiffPlatform = new UpdateGameTrackingCommand(fakeUserRemoteId, fakeGameRemoteId, fakeDiffPlatform,
            newFakeHoursPlayed, newFakeFormat, newFakeStatus, newFakeOwnership);
        var commandDiffUserAndGame = new UpdateGameTrackingCommand(fakeDiffUserRemoteId, fakeDiffGameRemoteId, fakePlatform,
            newFakeHoursPlayed, newFakeFormat, newFakeStatus, newFakeOwnership);
        var commandDiffGameAndPlatform = new UpdateGameTrackingCommand(fakeUserRemoteId, fakeDiffGameRemoteId, fakeDiffPlatform,
            newFakeHoursPlayed, newFakeFormat, newFakeStatus, newFakeOwnership);

        // Execute
        // Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
            UpdateGameTrackingHandler!.Handle(commandDiffUser, CancellationToken.None));
        await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
            UpdateGameTrackingHandler!.Handle(commandDiffGame, CancellationToken.None));
        await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
            UpdateGameTrackingHandler!.Handle(commandDiffPlatform, CancellationToken.None));
        await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
            UpdateGameTrackingHandler!.Handle(commandDiffUserAndGame, CancellationToken.None));
        await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
            UpdateGameTrackingHandler!.Handle(commandDiffGameAndPlatform, CancellationToken.None));
    }
}