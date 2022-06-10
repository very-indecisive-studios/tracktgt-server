using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Books.Content;
using Core.Exceptions;
using Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Persistence;
using Service.Book;

namespace Core.Test.Books.Content;

[TestClass]
public class GetBookTest
{
    private static SqliteConnection? Connection { get; set; }
    
    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }
    
    private static DatabaseContext? InMemDatabase { get; set; }
    
    private static Mock<IBookService>? MockBookService { get; set; }

    private static IMapper? Mapper { get; set; }
    
    private static GetBookHandler? GetBookHandler { get; set; }
    
    private static string fakeExistBookRemoteId = "d33zn4+5";
    
    [ClassInitialize]
    public static async Task TestClassInit(TestContext context)
    {
        var fakeBook = new Book()
        {
            RemoteId = fakeExistBookRemoteId,
            CoverImageURL = "https://chaoschef.example.com",
            Title = "Chaos Chef: Behind the Scenes",
            Summary = "Won Game of the Year",
            AuthorsString = "Very Indecisive Studios;Overflow"
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
        await InMemDatabase.SaveChangesAsync();
        
        MockBookService = new Mock<IBookService>();

        var mappingConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile<MappingProfiles>();
        });
        Mapper = mappingConfig.CreateMapper();

        GetBookHandler = new GetBookHandler(InMemDatabase, MockBookService.Object, Mapper);
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
    public async Task GetBook_Cached()
    {
        // Setup
        var query = new GetBookQuery(fakeExistBookRemoteId);

        // Execute
        await GetBookHandler!.Handle(query, CancellationToken.None);

        // Verify
        MockBookService!.VerifyNoOtherCalls();
    }
    
    [TestMethod]
    public async Task GetBook_NoCache()
    {
        // Setup
        string fakeId = "5u1iii1i1i11";
        
        var fakeAPIBook = new APIBook
        (
            fakeId,
            "https://tracktgt.xyz",
            "Tracktgt: Winning the Golden Boot",
            "Won book of the year and the golden book. It's morbin' time!",
            new List<string> { "Very Indecisive Studios", "Morbius" }
        );
        
        MockBookService!.Setup(service => service.GetBookById(fakeId))
            .ReturnsAsync(fakeAPIBook);
        
        var query = new GetBookQuery(fakeId);
        
        // Execute
        var result = await GetBookHandler!.Handle(query, CancellationToken.None);

        // Verify
        MockBookService.Verify(service => service.GetBookById(fakeId), Times.Once);

        Assert.IsTrue(await InMemDatabase!.Books.Where(b => b.RemoteId == fakeId).AnyAsync());
        
        Assert.AreEqual(result.RemoteId, fakeAPIBook.Id);
        Assert.AreEqual(result.CoverImageURL, fakeAPIBook.CoverImageURL);
        Assert.AreEqual(result.Title, fakeAPIBook.Title);
        Assert.AreEqual(result.Summary, fakeAPIBook.Summary);
        Assert.IsTrue(result.Authors.SequenceEqual(fakeAPIBook.Authors));
    }
    
    [TestMethod]
    public async Task GetBook_NotFound()
    {
        // Setup
        string fakeId = "bAS3dg0d";
        
        var query = new GetBookQuery(fakeId);
        

        MockBookService!.Setup(service => service.GetBookById(fakeId))
            .ReturnsAsync((APIBook?) null);
        
        // Execute
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => GetBookHandler!.Handle(query, CancellationToken.None));

        // Verify
        MockBookService.Verify(service => service.GetBookById(fakeId), Times.Once);
    }
}