using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Shows;
using Domain;
using Persistence;

namespace Core.Test.Shows;

[TestClass]
public class GetShowTrackingTest
{
    private static SqliteConnection? Connection { get; set; }

    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }

    private static DatabaseContext? InMemDatabase { get; set; }

    private static IMapper? Mapper { get; set; }

    private static GetShowTrackingHandler? GetShowTrackingHandler { get; set; }

    private const string FakeUserRemoteId = "d33Z_NuT5";
    private const string FakeShowRemoteId = "s_123";

    [ClassInitialize]
    public static async Task TestClassInit(TestContext context)
    {
        var fakeShowTrackingList = new List<ShowTracking>()
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
        InMemDatabase.ShowTrackings.AddRange(fakeShowTrackingList);
        await InMemDatabase.SaveChangesAsync();

        var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile<MappingProfiles>(); });
        Mapper = mappingConfig.CreateMapper();

        GetShowTrackingHandler = new GetShowTrackingHandler(InMemDatabase, Mapper);
    }

    [ClassCleanup]
    public static async Task TestClassCleanup()
    {
        await Connection!.DisposeAsync();
    }

    [TestMethod]
    public async Task GetShowTracking_Found()
    {
        // Setup
        var query = new GetShowTrackingQuery(FakeUserRemoteId, FakeShowRemoteId);

        // Execute
        var result = await GetShowTrackingHandler!.Handle(query, CancellationToken.None);
        
        // Verify
        Assert.IsNotNull(result);
    }
    
    [TestMethod]
    public async Task GetShowTracking_NotFound()
    {
        // Setup
        var query = new GetShowTrackingQuery(FakeUserRemoteId, "s_111");

        // Execute
        var result = await GetShowTrackingHandler!.Handle(query, CancellationToken.None);
        
        // Verify
        Assert.IsNull(result);
    }
}