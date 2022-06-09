using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Exceptions;
using Core.Shows;
using Domain;
using Persistence;

namespace Core.Test.Shows;

[TestClass]
public class UpdateShowTrackingTest
{
    private static SqliteConnection? Connection { get; set; }

    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }

    private static DatabaseContext? InMemDatabase { get; set; }

    private static UpdateShowTrackingHandler? UpdateShowTrackingHandler { get; set; }

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

        UpdateShowTrackingHandler = new UpdateShowTrackingHandler(InMemDatabase, Mapper);
    }

    [ClassCleanup]
    public static async Task TestClassCleanup()
    {
        await Connection!.DisposeAsync();
    }

    [TestMethod]
    public async Task UpdateShowTracking_Exists()
    {
        // Setup
        var fakeUserRemoteId = "d33Z_NuT5";
        var fakeShowRemoteId = "s_69";
        var fakeEpisodesWatched = 10;
        var fakeStatus = ShowTrackingStatus.Watching;
        var fakeShowType = ShowType.Series;
        InMemDatabase!.ShowTrackings.Add(new ShowTracking
        {
            UserRemoteId = fakeUserRemoteId,
            ShowRemoteId = fakeShowRemoteId,
            EpisodesWatched = fakeEpisodesWatched,
            Status = fakeStatus,
            ShowType = fakeShowType
        });
        await InMemDatabase.SaveChangesAsync(CancellationToken.None);

        // Simulated Update
        var newFakeEpisodesWatched = 16;
        var newFakeStatus = ShowTrackingStatus.Completed;

        var command = new UpdateShowTrackingCommand(fakeUserRemoteId, fakeShowRemoteId, newFakeEpisodesWatched, newFakeStatus);
        
        // Execute
         await UpdateShowTrackingHandler!.Handle(command, CancellationToken.None);
        
        // Verify
        var updatedShowTracking = await InMemDatabase.ShowTrackings
            .AsNoTracking()
            .Where(showTracking => showTracking.UserRemoteId == fakeUserRemoteId && showTracking.ShowRemoteId == fakeShowRemoteId)
            .FirstOrDefaultAsync(CancellationToken.None);
        Assert.IsNotNull(updatedShowTracking);
        Assert.AreEqual(updatedShowTracking.EpisodesWatched, newFakeEpisodesWatched);
        Assert.AreEqual(updatedShowTracking.Status, newFakeStatus);
    }

    [TestMethod]
    public async Task UpdateShowTracking_NotExists()
    {
        // Setup
        var fakeUserRemoteId = "d33Z_NuT5";
        var fakeDiffUserRemoteId = "d33Z_NuT5_L+M41d3nL3s5";
        var fakeShowRemoteId = "s_69";
        var fakeDiffShowRemoteId = "s_422";
        
        var newFakeEpisodesWatched = 25;
        var newFakeStatus = ShowTrackingStatus.Paused;
        
        var commandDiffUser = new UpdateShowTrackingCommand(fakeDiffUserRemoteId, fakeShowRemoteId, newFakeEpisodesWatched, newFakeStatus);
        var commandDiffShow = new UpdateShowTrackingCommand(fakeUserRemoteId, fakeDiffShowRemoteId, newFakeEpisodesWatched, newFakeStatus);
        var commandDiffUserAndShow = new UpdateShowTrackingCommand(fakeDiffUserRemoteId, fakeDiffShowRemoteId, newFakeEpisodesWatched, newFakeStatus);

        // Execute
        // Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
            UpdateShowTrackingHandler!.Handle(commandDiffUser, CancellationToken.None));
        await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
            UpdateShowTrackingHandler!.Handle(commandDiffShow, CancellationToken.None));
        await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
            UpdateShowTrackingHandler!.Handle(commandDiffUserAndShow, CancellationToken.None));
    }
}