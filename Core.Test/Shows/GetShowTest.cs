using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Shows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Core.Exceptions;
using Domain;
using Domain.Media;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Service.Show;

namespace Core.Test.Shows;

[TestClass]
public class GetShowTest
{
    private static SqliteConnection? Connection { get; set; }
    
    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }
    
    private static DatabaseContext? InMemDatabase { get; set; }
    
    private static Mock<IShowService>? MockShowService { get; set; }

    private static IMapper? Mapper { get; set; }
    
    private static GetShowHandler? GetShowHandler { get; set; }
    
    private static string fakeExistShowRemoteId = "m_38167";
    
    [ClassInitialize]
    public static async Task TestClassInit(TestContext context)
    {
        var fakeShow= new Show()
        {
            RemoteId = fakeExistShowRemoteId,
            CoverImageURL = "https://a24films.com/films/everything-everywhere-all-at-once",
            Title = "Best Movie of 2022",
            Summary = "Won Movie of the Year",
            ShowType = ShowType.Movie
        };
        
        // Setup in memory database
        Connection = new SqliteConnection("Filename=:memory:");
        Connection.Open();

        ContextOptions = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite(Connection)
            .Options;

        InMemDatabase = new DatabaseContext(ContextOptions);
        await InMemDatabase.Database.EnsureCreatedAsync();
        InMemDatabase.Shows.Add(fakeShow);
        await InMemDatabase.SaveChangesAsync();
        
        MockShowService = new Mock<IShowService>();

        var mappingConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile<MappingProfiles>();
        });
        Mapper = mappingConfig.CreateMapper();

        GetShowHandler = new GetShowHandler(InMemDatabase, MockShowService.Object, Mapper);
    }

    [ClassCleanup]
    public static async Task TestClassCleanup()
    {
        await Connection!.DisposeAsync();
    }
    
    [TestCleanup]
    public void TestCaseCleanup()
    {
        MockShowService.Reset();
    }
    
    [TestMethod]
    public async Task GetShow_Cached()
    {
        // Setup
        var query = new GetShowQuery(fakeExistShowRemoteId);

        // Execute
        await GetShowHandler!.Handle(query, CancellationToken.None);

        // Verify
        MockShowService!.VerifyNoOtherCalls();
    }
    
    [TestMethod]
    public async Task GetShow_NoCache()
    {
        // Setup
        string fakeId = "m_11231";
        ShowType fakeShowType = ShowType.Movie;
        
        var fakeAPIShow = new APIShow
        (
            fakeId,
            "https://tracktgt.xyz",
            "Tracktgt: Winning the Golden Boot",
            "Won show of It's morbin' time!",
            fakeShowType
        );
        
        MockShowService!.Setup(service => service.GetShowById(fakeId))
            .ReturnsAsync(fakeAPIShow);
        
        var query = new GetShowQuery(fakeId);
        
        // Execute
        var result = await GetShowHandler!.Handle(query, CancellationToken.None);

        // Verify
        MockShowService.Verify(service => service.GetShowById(fakeId), Times.Once);

        Assert.IsTrue(await InMemDatabase!.Shows.Where(s => s.RemoteId == fakeId).AnyAsync());
        
        Assert.AreEqual(result.RemoteId, fakeAPIShow.Id);
        Assert.AreEqual(result.CoverImageURL, fakeAPIShow.CoverImageURL);
        Assert.AreEqual(result.Title, fakeAPIShow.Title);
        Assert.AreEqual(result.Summary, fakeAPIShow.Summary);
        Assert.AreEqual(result.ShowType, fakeAPIShow.ShowType);
    }
    
    [TestMethod]
    public async Task GetShow_NotFound()
    {
        // Setup
        string fakeId = "m_123";
        
        var query = new GetShowQuery(fakeId);
        

        MockShowService!.Setup(service => service.GetShowById(fakeId))
            .ReturnsAsync((APIShow?) null);
        
        // Execute
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => GetShowHandler!.Handle(query, CancellationToken.None));

        // Verify
        MockShowService.Verify(service => service.GetShowById(fakeId), Times.Once);
    }
}