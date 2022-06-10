using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Exceptions;
using Core.Games.Wishlist;
using Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Persistence;
using Service.Game;

namespace Core.Test.Games.Wishlist;

[TestClass]
public class AddGameWishlistTest
{
    private static SqliteConnection? Connection { get; set; }
    
    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }
    
    private static DatabaseContext? InMemDatabase { get; set; }
    
    private static Mock<IGameService>? MockGameService { get; set; }

    private static IMapper? Mapper { get; set; }
    
    private static AddGameWishlistHandler? AddGameWishlistHandler { get; set; }

    private const long FakeExistingGameId = 1;
    private const string FakeExistingUserId = "USEREXIST";

    [ClassInitialize]
    public static async Task TestClassInit(TestContext context)
    {
        var fakeUser = new User()
        {
            RemoteId = FakeExistingUserId
        };

        var fakeGame = new Game()
        {
            RemoteId = FakeExistingGameId
        };
        
        // Setup in memory database
        Connection = new SqliteConnection("Filename=:memory:");
        Connection.Open();

        ContextOptions = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite(Connection)
            .Options;

        InMemDatabase = new DatabaseContext(ContextOptions);
        await InMemDatabase.Database.EnsureCreatedAsync();

        InMemDatabase.Games.Add(fakeGame);
        InMemDatabase.Users.Add(fakeUser);

        await InMemDatabase.SaveChangesAsync();

        MockGameService = new Mock<IGameService>();

        var mappingConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile<MappingProfiles>();
        });
        Mapper = mappingConfig.CreateMapper();

        AddGameWishlistHandler = new AddGameWishlistHandler(InMemDatabase, MockGameService.Object, Mapper);
    }

    [ClassCleanup]
    public static async Task TestClassCleanup()
    {
        await Connection!.DisposeAsync();
    }
    
    [TestCleanup]
    public void TestCaseCleanup()
    {
        MockGameService.Reset();
    }

    [TestMethod]
    public async Task AddGameWishlist_Cached()
    {
        // Setup
        var fakePlatform = "PC";
        var command = new AddGameWishlistCommand(
            FakeExistingUserId,
            FakeExistingGameId,
            fakePlatform
        );
        
        // Execute
        await AddGameWishlistHandler!.Handle(command, CancellationToken.None);
        
        // Verify
        MockGameService!.Verify(service => service.GetGameById(It.IsAny<long>()), Times.Never);
        var gameWishlist = await InMemDatabase!.GameWishlists
            .Where(gw => gw.GameRemoteId == FakeExistingGameId 
                         && gw.UserRemoteId.Equals(FakeExistingUserId)
                         && gw.Platform.Equals(fakePlatform))
            .CountAsync();
        Assert.AreEqual(1, gameWishlist);
    }
    
    [TestMethod]
    public async Task AddGameWishlist_NoCached_APIHit()
    {
        // Setup
        var fakeAPIGame = new APIGame(
            2,
            "http://image.example.com",
            "Chaos Chef",
            "Won Game of the Year Twice",
            100,
            new List<string> { "PC" },
            new List<string> { "Very Indecisive Studios" }
        );

        var fakePlatform = "PC";
        var command = new AddGameWishlistCommand(
            FakeExistingUserId,
            fakeAPIGame.Id,
            fakePlatform
        );
        
        MockGameService!.Setup(service => service.GetGameById(command.GameRemoteId))
            .ReturnsAsync(fakeAPIGame);
        
        // Execute
        await AddGameWishlistHandler!.Handle(command, CancellationToken.None);
        
        // Verify
        MockGameService.Verify(service => service.GetGameById(It.IsAny<long>()));
        var gameWishlistCount = await InMemDatabase!.GameWishlists
            .Where(gw => gw.GameRemoteId == FakeExistingGameId 
                         && gw.UserRemoteId.Equals(FakeExistingUserId)
                         && gw.Platform.Equals(fakePlatform))
            .CountAsync();
        Assert.AreEqual(1, gameWishlistCount);
        var gameCount = await InMemDatabase.Games
            .Where(g => g.RemoteId.Equals(fakeAPIGame.Id))
            .CountAsync();
        Assert.AreEqual(1, gameCount);
    }
    
    [TestMethod]
    public async Task AddGameWishlist_TrackingExists()
    {
        // Setup
        var fakePlatform = "PSVita";
        var command = new AddGameWishlistCommand(
            FakeExistingUserId,
            FakeExistingGameId,
            fakePlatform
        );

        // Execute 
        await AddGameWishlistHandler!.Handle(command, CancellationToken.None);
        
        // Verify
        await Assert.ThrowsExceptionAsync<ExistsException>(() => AddGameWishlistHandler.Handle(command, CancellationToken.None));
    }
    
    [TestMethod]
    public async Task AddGameWishlist_GameNotFound()
    {
        // Setup
        var fakePlatform = "PC";
        var command = new AddGameWishlistCommand(
            FakeExistingUserId,
            100000,
            fakePlatform
        );
        
        MockGameService!.Setup(service => service.GetGameById(command.GameRemoteId))
            .ReturnsAsync((APIGame?) null);
        
        // Execute & Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => AddGameWishlistHandler!.Handle(command, CancellationToken.None));
        MockGameService.Verify(service => service.GetGameById(It.IsAny<long>()));
    }
    
    [TestMethod]
    public async Task AddGameWishlist_UserNotFound()
    {
        // Setup
        var fakeAPIGame = new APIGame(
            2,
            "http://image.example.com",
            "Chaos Chef Remastered",
            "Won Game of the Year",
            100,
            new List<string> { "PC" },
            new List<string> { "Very Indecisive Studios" }
        );

        var fakePlatform = "PC";
        var command = new AddGameWishlistCommand(
            "abcd",
            fakeAPIGame.Id,
            fakePlatform
        );
        
        MockGameService!.Setup(service => service.GetGameById(command.GameRemoteId))
            .ReturnsAsync(fakeAPIGame);
        
        // Execute & Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => AddGameWishlistHandler!.Handle(command, CancellationToken.None));
    }
}