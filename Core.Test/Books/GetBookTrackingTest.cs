using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Books;
using Domain;
using Persistence;

namespace Core.Test.Books;

[TestClass]
public class GetBookTrackingTest
{
    private static SqliteConnection? Connection { get; set; }

    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }

    private static DatabaseContext? InMemDatabase { get; set; }

    private static IMapper? Mapper { get; set; }

    private static GetBookTrackingHandler? GetBookTrackingHandler { get; set; }

    private const string FakeUserRemoteId = "d33Z_NuT5";
    private const string FakeBookRemoteId = "0";

    [ClassInitialize]
    public static async Task TestClassInit(TestContext context)
    {
        var fakeBookTrackingsList = new List<BookTracking>()
        {
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                BookRemoteId = FakeBookRemoteId,
                ChaptersRead = 100,
                Format = BookTrackingFormat.Digital,
                Status = BookTrackingStatus.Reading,
                Ownership = BookTrackingOwnership.Wishlist
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
        InMemDatabase.BookTrackings.AddRange(fakeBookTrackingsList);
        await InMemDatabase.SaveChangesAsync();

        var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile<MappingProfiles>(); });
        Mapper = mappingConfig.CreateMapper();

        GetBookTrackingHandler = new GetBookTrackingHandler(InMemDatabase, Mapper);
    }

    [ClassCleanup]
    public static async Task TestClassCleanup()
    {
        await Connection!.DisposeAsync();
    }

    [TestMethod]
    public async Task GetBookTracking_Found()
    {
        // Setup
        var query = new GetBookTrackingQuery(FakeUserRemoteId, FakeBookRemoteId);

        // Execute
        var result = await GetBookTrackingHandler!.Handle(query, CancellationToken.None);
        
        // Verify
        Assert.IsNotNull(result);
    }
    
    [TestMethod]
    public async Task GetBookTracking_NotFound()
    {
        // Setup
        var query = new GetBookTrackingQuery(FakeUserRemoteId, "BOOKNOTEXIST");

        // Execute
        var result = await GetBookTrackingHandler!.Handle(query, CancellationToken.None);
        
        // Verify
        Assert.IsNull(result);
    }
}