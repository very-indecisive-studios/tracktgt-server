using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Core.Exceptions;
using Core.Shows;
using Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Service.Show;

namespace Core.Test.Shows;

[TestClass]
public class AddShowTrackingTest
{
    private static SqliteConnection? Connection { get; set; }
    
    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }
    
    private static DatabaseContext? InMemDatabase { get; set; }
    
    private static Mock<IShowService>? MockShowService { get; set; }

    private static IMapper? Mapper { get; set; }
    
    private static AddShowTrackingHandler? AddShowTrackingHandler { get; set; }

    private const string FakeExistingShowId = "m_123";
    private const string FakeExistingUserId = "USEREXIST";

    [ClassInitialize]
    public static async Task TestClassInit(TestContext context)
    {
        var fakeUser = new User()
        {
            RemoteId = FakeExistingUserId
        };

        var fakeShow = new Show()
        {
            RemoteId = FakeExistingShowId
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
        InMemDatabase.Users.Add(fakeUser);

        await InMemDatabase.SaveChangesAsync();

        MockShowService = new Mock<IShowService>();

        var mappingConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile<MappingProfiles>();
        });
        Mapper = mappingConfig.CreateMapper();

        AddShowTrackingHandler = new AddShowTrackingHandler(InMemDatabase, MockShowService.Object, Mapper);
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
    public async Task AddShowTracking_Cached()
    {
        // Setup
        var command = new AddShowTrackingCommand(
            FakeExistingUserId,
            FakeExistingShowId,
            200,
            ShowType.Movie,
            ShowTrackingStatus.Completed
        );
        
        // Execute
        await AddShowTrackingHandler!.Handle(command, CancellationToken.None);
        
        // Verify
        MockShowService!.Verify(service => service.GetShowById(It.IsAny<string>()), Times.Never);
        var showTracking = await InMemDatabase!.ShowTrackings
            .Where(showTracking => showTracking.ShowRemoteId.Equals(FakeExistingShowId) 
                                   && showTracking.UserRemoteId.Equals(FakeExistingUserId))
            .CountAsync();
        Assert.AreEqual(1, showTracking);
    }
    
    [TestMethod]
    public async Task AddShowTracking_NoCached_APIHit()
    {
        // Setup
        var fakeAPIShow = new APIShow(
            "m_1223",
            "",
            "Chaos Chef",
            "Underrated Movie of the Year",
            ShowType.Movie
        );

        var command = new AddShowTrackingCommand(
            FakeExistingUserId,
            fakeAPIShow.Id,
            1,
            ShowType.Movie,
            ShowTrackingStatus.Completed
        );
        
        MockShowService!.Setup(service => service.GetShowById(command.ShowRemoteId))
            .ReturnsAsync(fakeAPIShow);
        
        // Execute
        await AddShowTrackingHandler!.Handle(command, CancellationToken.None);
        
        // Verify
        MockShowService.Verify(service => service.GetShowById(It.IsAny<string>()));
        var showTrackingCount = await InMemDatabase!.ShowTrackings
            .Where(showTracking => showTracking.ShowRemoteId.Equals(FakeExistingShowId) 
                                   && showTracking.UserRemoteId.Equals(FakeExistingUserId))
            .CountAsync();
        Assert.AreEqual(1, showTrackingCount);
        var showCount = await InMemDatabase.Shows
            .Where(b => b.RemoteId.Equals(fakeAPIShow.Id))
            .CountAsync();
        Assert.AreEqual(1, showCount);
    }
    
    [TestMethod]
    public async Task AddShowTracking_TrackingExists()
    {
        // Setup
        var command = new AddShowTrackingCommand(
            FakeExistingUserId,
            FakeExistingShowId,
            0,
            ShowType.Series,
            ShowTrackingStatus.Planning
        );
        
        MockShowService!.Setup(service => service.GetShowById(command.ShowRemoteId))
            .ReturnsAsync((APIShow?) null);
        
        // Execute & Verify
        await Assert.ThrowsExceptionAsync<ExistsException>(() => AddShowTrackingHandler!.Handle(command, CancellationToken.None));
    }
    
    [TestMethod]
    public async Task AddShowTracking_ShowNotFound()
    {
        // Setup
        var command = new AddShowTrackingCommand(
            FakeExistingUserId,
            "m_111",
            0,
            ShowType.Series,
            ShowTrackingStatus.Planning
        );
        
        MockShowService!.Setup(service => service.GetShowById(command.ShowRemoteId))
            .ReturnsAsync((APIShow?) null);
        
        // Execute & Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => AddShowTrackingHandler!.Handle(command, CancellationToken.None));
        MockShowService.Verify(service => service.GetShowById(It.IsAny<string>()));
    }
    
    [TestMethod]
    public async Task AddShowTracking_UserNotFound()
    {
        // Setup
        var fakeAPIShow = new APIShow(
            "m_123",
            "",
            "Chaos Chef",
            "Won Movie of the Year",
            ShowType.Movie
        );

        var command = new AddShowTrackingCommand(
            "abcd",
            fakeAPIShow.Id,
            200,
            ShowType.Movie,
            ShowTrackingStatus.Planning
        );

        
        MockShowService!.Setup(service => service.GetShowById(command.ShowRemoteId))
            .ReturnsAsync(fakeAPIShow);
        
        // Execute & Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => AddShowTrackingHandler!.Handle(command, CancellationToken.None));
    }
}