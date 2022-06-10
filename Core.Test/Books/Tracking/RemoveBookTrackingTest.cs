using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Books.Tracking;
using Core.Exceptions;
using Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Persistence;

namespace Core.Test.Books.Tracking;

[TestClass]
public class RemoveBookTrackingTest
{
    private static SqliteConnection? Connection { get; set; }

    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }

    private static DatabaseContext? InMemDatabase { get; set; }

    private static IMapper? Mapper { get; set; }

    private static RemoveBookTrackingHandler? RemoveBookTrackingHandler { get; set; }

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
                Ownership = BookTrackingOwnership.Owned
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

        RemoveBookTrackingHandler = new RemoveBookTrackingHandler(InMemDatabase);
    }

    [TestMethod]
    public async Task RemoveBookTracking_Exists()
    {
        // Setup
        var command = new RemoveBookTrackingCommand(FakeUserRemoteId, FakeBookRemoteId);
        
        // Execute
        await RemoveBookTrackingHandler!.Handle(command, CancellationToken.None);

        // Verify
        var count = await InMemDatabase!.BookTrackings
            .Where(b => b.UserRemoteId.Equals(FakeUserRemoteId) 
                        && b.BookRemoteId.Equals(FakeBookRemoteId))
            .CountAsync();
        Assert.AreEqual(0, count);
    }

    [TestMethod]
    public async Task RemoveBookTracking_NotExists()
    {
        // Setup
        var command = new RemoveBookTrackingCommand(FakeUserRemoteId, FakeBookRemoteId);

        // Execute
        // Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => 
            RemoveBookTrackingHandler!.Handle(command, CancellationToken.None));
    }
}