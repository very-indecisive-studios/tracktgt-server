using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Exceptions;
using Core.Shows;
using Domain;
using Domain.Media;
using Domain.Tracking;
using Domain.User;
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

        var mappingConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile<MappingProfiles>();
        });
        Mapper = mappingConfig.CreateMapper();

        AddShowTrackingHandler = new AddShowTrackingHandler(InMemDatabase, Mapper);
    }

    [ClassCleanup]
    public static async Task TestClassCleanup()
    {
        await Connection!.DisposeAsync();
    }

    [TestMethod]
    public async Task AddShowTracking_Default()
    {
        // Setup
        var command = new AddShowTrackingCommand(
            FakeExistingUserId,
            FakeExistingShowId,
            200,
            ShowTrackingStatus.Completed
        );
        
        // Execute
        await AddShowTrackingHandler!.Handle(command, CancellationToken.None);
        
        // Verify
        var showTracking = await InMemDatabase!.ShowTrackings
            .Where(showTracking => showTracking.ShowRemoteId.Equals(FakeExistingShowId) 
                                   && showTracking.UserRemoteId.Equals(FakeExistingUserId))
            .CountAsync();
        Assert.AreEqual(1, showTracking);
        
        var activity = await InMemDatabase.Activities
            .Where(a => a.UserRemoteId.Equals(FakeExistingUserId))
            .FirstOrDefaultAsync();
        Assert.IsNotNull(activity);
        Assert.AreEqual(ActivityMediaType.Show, activity.MediaType);
        Assert.AreEqual(ActivityAction.Add, activity.Action);
    }

    [TestMethod]
    public async Task AddShowTracking_TrackingExists()
    {
        // Setup
        var command = new AddShowTrackingCommand(
            FakeExistingUserId,
            FakeExistingShowId,
            0,
            ShowTrackingStatus.Planning
        );

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
            ShowTrackingStatus.Planning
        );
        
        // Execute & Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => AddShowTrackingHandler!.Handle(command, CancellationToken.None));
    }
    
    [TestMethod]
    public async Task AddShowTracking_UserNotFound()
    {
        var command = new AddShowTrackingCommand(
            "abcd",
            "s_shownotexist",
            200,
            ShowTrackingStatus.Planning
        );

        // Execute & Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => AddShowTrackingHandler!.Handle(command, CancellationToken.None));
    }
}