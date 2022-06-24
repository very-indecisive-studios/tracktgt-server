using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Books.Tracking;
using Domain;
using Domain.Media;
using Domain.Tracking;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Persistence;

namespace Core.Test.Books.Tracking;

[TestClass]
public class GetAllBookTrackingsTest
{
    private static SqliteConnection? Connection { get; set; }
    
    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }
    
    private static DatabaseContext? InMemDatabase { get; set; }

    private static IMapper? Mapper { get; set; }

    private static GetAllBookTrackingsHandler? GetAllBookTrackingsHandler { get; set; }

    private const string FakeUserRemoteId = "d33Z_NuT5";
    private const string FakeDiffUserRemoteId = "d33Z_NuT5+L+M41d3Nl35S";

    [ClassInitialize]
    public static async Task TestClassInit(TestContext context)
    {
        var fakeBookTrackingsList = new List<BookTracking>()
        {
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                BookRemoteId = "0",
                ChaptersRead = 100,
                Format = BookTrackingFormat.Digital,
                Status = BookTrackingStatus.Reading,
                Ownership = BookTrackingOwnership.Owned
            },
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                BookRemoteId = "1",
                ChaptersRead = 90,
                Format = BookTrackingFormat.Digital,
                Status = BookTrackingStatus.Reading,
                Ownership = BookTrackingOwnership.Owned
            },
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                BookRemoteId = "2",
                ChaptersRead = 80,
                Format = BookTrackingFormat.Physical,
                Status = BookTrackingStatus.Paused,
                Ownership = BookTrackingOwnership.Owned
            },
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                BookRemoteId = "3",
                ChaptersRead = 70,
                Format = BookTrackingFormat.Physical,
                Status = BookTrackingStatus.Planning,
                Ownership = BookTrackingOwnership.Loan
            },
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                BookRemoteId = "4",
                ChaptersRead = 60,
                Format = BookTrackingFormat.Digital,
                Status = BookTrackingStatus.Planning,
                Ownership = BookTrackingOwnership.Owned
            },
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                BookRemoteId = "5",
                ChaptersRead = 50,
                Format = BookTrackingFormat.Physical,
                Status = BookTrackingStatus.Completed,
                Ownership = BookTrackingOwnership.Loan
            },
            new()
            {
                UserRemoteId = FakeDiffUserRemoteId,
                BookRemoteId = "6",
                ChaptersRead = 25,
                Format = BookTrackingFormat.Physical,
                Status = BookTrackingStatus.Paused,
                Ownership = BookTrackingOwnership.Loan
            }
        };

        var fakeBooksList = new List<Book>()
        {
            new() { RemoteId = "0" },
            new() { RemoteId = "1" },
            new() { RemoteId = "2" },
            new() { RemoteId = "3" },
            new() { RemoteId = "4" },
            new() { RemoteId = "5" },
            new() { RemoteId = "6" },
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
        InMemDatabase.Books.AddRange(fakeBooksList);
        await InMemDatabase.SaveChangesAsync();

        var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile<MappingProfiles>(); });
        Mapper = mappingConfig.CreateMapper();

        GetAllBookTrackingsHandler = new GetAllBookTrackingsHandler(InMemDatabase);
    }

    [ClassCleanup]
    public static async Task TestClassCleanup()
    {
        await Connection!.DisposeAsync();
    }


    [TestMethod]
    public async Task GetAllBookTrackings_Default()
    {
        // Setup
        var query = new GetAllBookTrackingsQuery()
        {
            UserRemoteId = FakeUserRemoteId,
        };

        // Execute
        var result = await GetAllBookTrackingsHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual(6, result.TotalCount);
    }

    [TestMethod]
    public async Task GetAllBookTrackings_ByBookStatus()
    {
        // Setup
        var queryCompleted = new GetAllBookTrackingsQuery()
        {
            UserRemoteId = FakeUserRemoteId,
            BookStatus = BookTrackingStatus.Completed,
        };
        var queryReading = new GetAllBookTrackingsQuery()
        {
            UserRemoteId = FakeUserRemoteId,
            BookStatus = BookTrackingStatus.Reading,
        };
        var queryPaused = new GetAllBookTrackingsQuery()
        {
            UserRemoteId = FakeUserRemoteId,
            BookStatus = BookTrackingStatus.Paused,
        };
        var queryPlanning = new GetAllBookTrackingsQuery()
        {
            UserRemoteId = FakeUserRemoteId,
            BookStatus = BookTrackingStatus.Planning,
        };

        // Execute
        var resultCompleted = await GetAllBookTrackingsHandler!.Handle(queryCompleted, CancellationToken.None);
        var resultPlaying = await GetAllBookTrackingsHandler.Handle(queryReading, CancellationToken.None);
        var resultPaused = await GetAllBookTrackingsHandler.Handle(queryPaused, CancellationToken.None);
        var resultPlanning = await GetAllBookTrackingsHandler.Handle(queryPlanning, CancellationToken.None);

        // Verify
        Assert.AreEqual(1, resultCompleted.TotalCount);
        Assert.AreEqual(2, resultPlaying.TotalCount);
        Assert.AreEqual(1, resultPaused.TotalCount);
        Assert.AreEqual(2, resultPlanning.TotalCount);
    }

    [TestMethod]
    public async Task GetAllBookTrackings_SortByChaptersRead()
    {
        // Setup
        var query = new GetAllBookTrackingsQuery()
        {
            UserRemoteId = FakeUserRemoteId,
            SortByChaptersRead = true
        };

        // Execute
        var result = await GetAllBookTrackingsHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual(6, result.TotalCount);
        Assert.AreEqual(50, result.Items.First().ChaptersRead);
        Assert.AreEqual(100, result.Items.Last().ChaptersRead);
    }

    [TestMethod]
    public async Task GetAllBookTrackings_SortByFormat()
    {
        // Setup
        var query = new GetAllBookTrackingsQuery()
        {
            UserRemoteId = FakeUserRemoteId,
            SortByFormat = true
        };

        // Execute
        var result = await GetAllBookTrackingsHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual(6, result.TotalCount);
        Assert.AreEqual(BookTrackingFormat.Digital, result.Items.First().Format);
        Assert.AreEqual(BookTrackingFormat.Physical, result.Items.Last().Format);
    }

    [TestMethod]
    public async Task GetAllBookTrackings_SortByOwnership()
    {
        // Setup
        var query = new GetAllBookTrackingsQuery()
        {
            UserRemoteId = FakeUserRemoteId,
            SortByOwnership = true
        };

        // Execute
        var result = await GetAllBookTrackingsHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual(6, result.TotalCount);
        Assert.AreEqual(BookTrackingOwnership.Owned, result.Items.First().Ownership);
        Assert.AreEqual(BookTrackingOwnership.Loan, result.Items.Last().Ownership);
    }
    
    [TestMethod]
    public async Task GetAllBookTrackings_SortByRecentlyModified()
    {
        // This test modifies the database and should be last to run.
        
        // Setup
        var query = new GetAllBookTrackingsQuery()
        {
            UserRemoteId = FakeUserRemoteId,
            SortByRecentlyModified = true
        };
        
        var recentlyModifiedBookIdList = new List<string>();
        var bookTrackings = await InMemDatabase!.BookTrackings
            .Where(gt => gt.UserRemoteId == FakeUserRemoteId)
            .ToListAsync();
        foreach (var bookTracking in bookTrackings)
        {
            recentlyModifiedBookIdList.Add(bookTracking.BookRemoteId);
            bookTracking.ChaptersRead += 1;
            InMemDatabase.Update(bookTracking);
            await InMemDatabase.SaveChangesAsync();
            await Task.Delay(1000);
        }

        // Execute
        var result = await GetAllBookTrackingsHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual(6, result.TotalCount);
        Assert.AreEqual(recentlyModifiedBookIdList.Last(), result.Items.First().BookRemoteId);
        Assert.AreEqual(recentlyModifiedBookIdList.First(), result.Items.Last().BookRemoteId);
    }
}