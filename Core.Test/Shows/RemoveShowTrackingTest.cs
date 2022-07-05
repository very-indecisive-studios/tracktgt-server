using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Exceptions;
using Core.Shows;
using Domain;
using Domain.Media;
using Domain.Tracking;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Test.Shows;

[TestClass]
public class RemoveShowTrackingTest
{
    private static SqliteConnection? Connection { get; set; }

    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }

    private static DatabaseContext? InMemDatabase { get; set; }

    private static IMapper? Mapper { get; set; }

    private static RemoveShowTrackingHandler? RemoveShowTrackingHandler { get; set; }

    private const string FakeUserRemoteId = "d33Z_NuT5";
    private const string FakeShowRemoteId = "s_123";

    [ClassInitialize]
    public static async Task TestClassInit(TestContext context)
    {
        var fakeShow = new Show()
        {
            RemoteId = FakeShowRemoteId
        };
        
        var fakeShowTrackingsList = new List<ShowTracking>()
        {
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                ShowRemoteId = FakeShowRemoteId,
                EpisodesWatched = 123,
                Status = ShowTrackingStatus.Watching
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
        InMemDatabase.ShowTrackings.AddRange(fakeShowTrackingsList);
        InMemDatabase.Shows.Add(fakeShow);
        await InMemDatabase.SaveChangesAsync();

        var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile<MappingProfiles>(); });
        Mapper = mappingConfig.CreateMapper();

        RemoveShowTrackingHandler = new RemoveShowTrackingHandler(InMemDatabase);
    }

    [TestMethod]
    public async Task RemoveShowTracking_Exists()
    {
        // Setup
        var command = new RemoveShowTrackingCommand(FakeUserRemoteId, FakeShowRemoteId);
        
        // Execute
        await RemoveShowTrackingHandler!.Handle(command, CancellationToken.None);

        // Verify
        var count = await InMemDatabase!.ShowTrackings
            .Where(showTracking => showTracking.UserRemoteId.Equals(FakeUserRemoteId)
                                   && showTracking.ShowRemoteId.Equals(FakeShowRemoteId))
            .CountAsync();
        Assert.AreEqual(0, count);
        
        var activity = await InMemDatabase.Activities
            .Where(a => a.UserRemoteId.Equals(FakeUserRemoteId))
            .FirstOrDefaultAsync();
        Assert.IsNotNull(activity);
        Assert.AreEqual(ActivityMediaType.Show, activity.MediaType);
        Assert.AreEqual(ActivityAction.Remove, activity.Action);
    }

    [TestMethod]
    public async Task RemoveShowTracking_NotExists()
    {
        // Setup
        var command = new RemoveShowTrackingCommand(FakeUserRemoteId, FakeShowRemoteId);

        // Execute
        // Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => 
            RemoveShowTrackingHandler!.Handle(command, CancellationToken.None));
    }
}