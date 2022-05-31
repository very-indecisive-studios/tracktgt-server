using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Core.Exceptions;
using Core.Books;
using Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Service.Book;

namespace Core.Test.Books;

[TestClass]
public class AddBookTrackingTest
{
    private static SqliteConnection? Connection { get; set; }
    
    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }
    
    private static DatabaseContext? InMemDatabase { get; set; }
    
    private static Mock<IBookService>? MockBookService { get; set; }

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

        MockBookService = new Mock<IBookService>();

        var mappingConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile<MappingProfiles>();
        });
        Mapper = mappingConfig.CreateMapper();

        AddBookTrackingHandler = new AddBookTrackingHandler(InMemDatabase, MockBookService.Object, Mapper);
    }

    [ClassCleanup]
    public static async Task TestClassCleanup()
    {
        await Connection!.DisposeAsync();
    }
    
    [TestCleanup]
    public void TestCaseCleanup()
    {
        MockBookService.Reset();
    }

    [TestMethod]
    public async Task AddBookTracking_Cached()
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
        MockBookService!.Verify(service => service.GetBookById(It.IsAny<string>()), Times.Never);
        var bookTracking = await InMemDatabase.BookTrackings
            .Where(bt => bt.BookRemoteId.Equals(FakeExistingBookId) 
                         && bt.UserRemoteId.Equals(FakeExistingUserId))
            .CountAsync();
        Assert.AreEqual(1, bookTracking);
    }
    
    [TestMethod]
    public async Task AddBookTracking_NoCached_APIHit()
    {
        // Setup
        var fakeAPIBook = new APIBook(
            "BOOKNOTEXIST",
            "",
            "Chaos Chef",
            "Won Book of the Year",
            new List<string> { "Very Indecisive Studios" }
        );

        var command = new AddBookTrackingCommand(
            FakeExistingUserId,
            fakeAPIBook.Id,
            200,
            BookTrackingFormat.Digital,
            BookTrackingStatus.Planning,
            BookTrackingOwnership.Owned
        );
        
        MockBookService!.Setup(service => service.GetBookById(command.BookRemoteId))
            .ReturnsAsync(fakeAPIBook);
        
        // Execute
        await AddBookTrackingHandler!.Handle(command, CancellationToken.None);
        
        // Verify
        MockBookService.Verify(service => service.GetBookById(It.IsAny<string>()));
        var bookTrackingCount = await InMemDatabase.BookTrackings
            .Where(bt => bt.BookRemoteId.Equals(FakeExistingBookId) 
                         && bt.UserRemoteId.Equals(FakeExistingUserId))
            .CountAsync();
        Assert.AreEqual(1, bookTrackingCount);
        var bookCount = await InMemDatabase.Books
            .Where(b => b.RemoteId.Equals(fakeAPIBook.Id))
            .CountAsync();
        Assert.AreEqual(1, bookCount);
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
        
        MockBookService!.Setup(service => service.GetBookById(command.BookRemoteId))
            .ReturnsAsync((APIBook?) null);
        
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
        
        MockBookService!.Setup(service => service.GetBookById(command.BookRemoteId))
            .ReturnsAsync((APIBook?) null);
        
        // Execute & Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => AddBookTrackingHandler!.Handle(command, CancellationToken.None));
        MockBookService.Verify(service => service.GetBookById(It.IsAny<string>()));
    }
    
    [TestMethod]
    public async Task AddBookTracking_UserNotFound()
    {
        // Setup
        var fakeAPIBook = new APIBook(
            "2BOOKNOTEXIST",
            "",
            "Chaos Chef",
            "Won Book of the Year",
            new List<string> { "Very Indecisive Studios" }
        );

        var command = new AddBookTrackingCommand(
            "abcd",
            fakeAPIBook.Id,
            200,
            BookTrackingFormat.Digital,
            BookTrackingStatus.Planning,
            BookTrackingOwnership.Owned
        );

        
        MockBookService!.Setup(service => service.GetBookById(command.BookRemoteId))
            .ReturnsAsync(fakeAPIBook);
        
        // Execute & Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => AddBookTrackingHandler!.Handle(command, CancellationToken.None));
    }
}