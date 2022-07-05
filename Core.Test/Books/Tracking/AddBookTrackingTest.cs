using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Books.Tracking;
using Core.Exceptions;
using Domain.Media;
using Domain.Tracking;
using Domain.User;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Persistence;
using Service.Book;

namespace Core.Test.Books.Tracking;

[TestClass]
public class AddBookTrackingTest
{
    private static SqliteConnection? Connection { get; set; }
    
    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }
    
    private static DatabaseContext? InMemDatabase { get; set; }

    private static IMapper? Mapper { get; set; }
    
    private static AddBookTrackingHandler? AddBookTrackingHandler { get; set; }

    private const string FakeExistingBookId = "BOOKEXIST";
    private const string FakeExistingUserId = "USEREXIST";

    [ClassInitialize]
    public static async Task TestClassInit(TestContext context)
    {
        var fakeUser = new User()
        {
            RemoteId = FakeExistingUserId
        };

        var fakeBook = new Book()
        {
            RemoteId = FakeExistingBookId
        };
        
        // Setup in memory database
        Connection = new SqliteConnection("Filename=:memory:");
        Connection.Open();

        ContextOptions = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite(Connection)
            .Options;

        InMemDatabase = new DatabaseContext(ContextOptions);
        await InMemDatabase.Database.EnsureCreatedAsync();

        InMemDatabase.Books.Add(fakeBook);
        InMemDatabase.Users.Add(fakeUser);

        await InMemDatabase.SaveChangesAsync();

        var mappingConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile<MappingProfiles>();
        });
        Mapper = mappingConfig.CreateMapper();

        AddBookTrackingHandler = new AddBookTrackingHandler(InMemDatabase, Mapper);
    }

    [ClassCleanup]
    public static async Task TestClassCleanup()
    {
        await Connection!.DisposeAsync();
    }

    [TestMethod]
    public async Task AddBookTracking_Default()
    {
        // Setup
        var command = new AddBookTrackingCommand(
            FakeExistingUserId,
            FakeExistingBookId,
            200,
            BookTrackingFormat.Digital,
            BookTrackingStatus.Planning,
            BookTrackingOwnership.Owned
        );
        
        // Execute
        await AddBookTrackingHandler!.Handle(command, CancellationToken.None);
        
        // Verify
        var bookTracking = await InMemDatabase!.BookTrackings
            .Where(bt => bt.BookRemoteId.Equals(FakeExistingBookId) 
                         && bt.UserRemoteId.Equals(FakeExistingUserId))
            .CountAsync();
        Assert.AreEqual(1, bookTracking);
    }

    [TestMethod]
    public async Task AddBookTracking_TrackingExists()
    {
        // Setup
        var command = new AddBookTrackingCommand(
            FakeExistingUserId,
            FakeExistingBookId,
            200,
            BookTrackingFormat.Digital,
            BookTrackingStatus.Planning,
            BookTrackingOwnership.Owned
        );

        // Execute & Verify
        await Assert.ThrowsExceptionAsync<ExistsException>(() => AddBookTrackingHandler!.Handle(command, CancellationToken.None));
    }
    
    [TestMethod]
    public async Task AddBookTracking_BookNotFound()
    {
        // Setup
        var command = new AddBookTrackingCommand(
            FakeExistingUserId,
            "BOOKNOTVALID",
            200,
            BookTrackingFormat.Digital,
            BookTrackingStatus.Planning,
            BookTrackingOwnership.Owned
        );
        
        // Execute & Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => AddBookTrackingHandler!.Handle(command, CancellationToken.None));
    }
    
    [TestMethod]
    public async Task AddBookTracking_UserNotFound()
    {
        var command = new AddBookTrackingCommand(
            "abcd",
            "does not exist",
            200,
            BookTrackingFormat.Digital,
            BookTrackingStatus.Planning,
            BookTrackingOwnership.Owned
        );

        // Execute & Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => AddBookTrackingHandler!.Handle(command, CancellationToken.None));
    }
}