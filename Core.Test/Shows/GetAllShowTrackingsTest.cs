using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Shows;
using Domain;
using Domain.Media;
using Domain.Tracking;
using Persistence;

namespace Core.Test.Shows;

[TestClass]
public class GetAllShowTrackingsTest
{
    private static SqliteConnection? Connection { get; set; }
    
    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }
    
    private static DatabaseContext? InMemDatabase { get; set; }

    private static IMapper? Mapper { get; set; }

    private static GetAllShowTrackingsHandler? GetAllShowTrackingsHandler { get; set; }

    private const string FakeUserRemoteId = "d33Z_NuT5";
    private const string FakeDiffUserRemoteId = "d33Z_NuT5+L+M41d3Nl35S";

    [ClassInitialize]
    public static async Task TestClassInit(TestContext context)
    {
        var fakeShowTrackingsList = new List<ShowTracking>()
        {
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                ShowRemoteId = "s_0",
                EpisodesWatched = 100,
                ShowType = ShowType.Series,
                Status = ShowTrackingStatus.Watching
            },
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                ShowRemoteId = "m_1",
                EpisodesWatched = 1,
                ShowType = ShowType.Movie,
                Status = ShowTrackingStatus.Completed
            },
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                ShowRemoteId = "s_2",
                EpisodesWatched = 16,
                ShowType = ShowType.Series,
                Status = ShowTrackingStatus.Completed
            },
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                ShowRemoteId = "m_3",
                EpisodesWatched = 1,
                ShowType = ShowType.Movie,
                Status = ShowTrackingStatus.Completed
            },
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                ShowRemoteId = "m_4",
                EpisodesWatched = 0,
                ShowType = ShowType.Movie,
                Status = ShowTrackingStatus.Planning
            },
            new()
            {
                UserRemoteId = FakeDiffUserRemoteId,
                ShowRemoteId = "m_5",
                EpisodesWatched = 1,
                ShowType = ShowType.Movie,
                Status = ShowTrackingStatus.Completed
            }
        };

        var fakeShowsList = new List<Show>()
        {
            new() { RemoteId = "s_0" },
            new() { RemoteId = "m_1" },
            new() { RemoteId = "s_2" },
            new() { RemoteId = "m_3" },
            new() { RemoteId = "m_4" },
            new() { RemoteId = "m_5" },
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
        InMemDatabase.Shows.AddRange(fakeShowsList);
        await InMemDatabase.SaveChangesAsync();

        var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile<MappingProfiles>(); });
        Mapper = mappingConfig.CreateMapper();

        GetAllShowTrackingsHandler = new GetAllShowTrackingsHandler(InMemDatabase, Mapper);
    }

    [ClassCleanup]
    public static async Task TestClassCleanup()
    {
        await Connection!.DisposeAsync();
    }


    [TestMethod]
    public async Task GetAllShowTrackings_Default()
    {
        // Setup
        var query = new GetAllShowTrackingsQuery()
        {
            UserRemoteId = FakeUserRemoteId,
        };

        // Execute
        var result = await GetAllShowTrackingsHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual(5, result.TotalCount);
    }

    [TestMethod]
    public async Task GetAllShowTrackings_ByShowStatus()
    {
        // Setup
        var queryCompleted = new GetAllShowTrackingsQuery()
        {
            UserRemoteId = FakeUserRemoteId,
            Status = ShowTrackingStatus.Completed,
        };
        var queryWatching = new GetAllShowTrackingsQuery()
        {
            UserRemoteId = FakeUserRemoteId,
            Status = ShowTrackingStatus.Watching,
        };
        var queryPaused = new GetAllShowTrackingsQuery()
        {
            UserRemoteId = FakeUserRemoteId,
            Status = ShowTrackingStatus.Paused,
        };
        var queryPlanning = new GetAllShowTrackingsQuery()
        {
            UserRemoteId = FakeUserRemoteId,
            Status = ShowTrackingStatus.Planning,
        };

        // Execute
        var resultCompleted = await GetAllShowTrackingsHandler!.Handle(queryCompleted, CancellationToken.None);
        var resultWatching = await GetAllShowTrackingsHandler.Handle(queryWatching, CancellationToken.None);
        var resultPaused = await GetAllShowTrackingsHandler.Handle(queryPaused, CancellationToken.None);
        var resultPlanning = await GetAllShowTrackingsHandler.Handle(queryPlanning, CancellationToken.None);

        // Verify
        Assert.AreEqual(3, resultCompleted.TotalCount);
        Assert.AreEqual(1, resultWatching.TotalCount);
        Assert.AreEqual(0, resultPaused.TotalCount);
        Assert.AreEqual(1, resultPlanning.TotalCount);
    }

    [TestMethod]
    public async Task GetAllShowTrackings_SortByEpisodesWatched()
    {
        // Setup
        var query = new GetAllShowTrackingsQuery()
        {
            UserRemoteId = FakeUserRemoteId,
            SortByEpisodesWatched = true
        };

        // Execute
        var result = await GetAllShowTrackingsHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual(5, result.TotalCount);
        Assert.AreEqual(0, result.Items.First().EpisodesWatched);
        Assert.AreEqual(100, result.Items.Last().EpisodesWatched);
    }

    [TestMethod]
    public async Task GetAllShowTrackings_SortByRecentlyModified()
    {
        // This test modifies the database and should be last to run.
        
        // Setup
        var query = new GetAllShowTrackingsQuery()
        {
            UserRemoteId = FakeUserRemoteId,
            SortByRecentlyModified = true
        };
        
        var recentlyModifiedShowIdList = new List<string>();
        var showTrackings = await InMemDatabase!.ShowTrackings
            .Where(st => st.UserRemoteId == FakeUserRemoteId)
            .ToListAsync();
        foreach (var showTracking in showTrackings)
        {
            recentlyModifiedShowIdList.Add(showTracking.ShowRemoteId);
            showTracking.EpisodesWatched += 1;
            InMemDatabase.Update(showTracking);
            await InMemDatabase.SaveChangesAsync();
            await Task.Delay(1000);
        }

        // Execute
        var result = await GetAllShowTrackingsHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual(5, result.TotalCount);
        Assert.AreEqual(recentlyModifiedShowIdList.Last(), result.Items.First().ShowRemoteId);
        Assert.AreEqual(recentlyModifiedShowIdList.First(), result.Items.Last().ShowRemoteId);
    }
}