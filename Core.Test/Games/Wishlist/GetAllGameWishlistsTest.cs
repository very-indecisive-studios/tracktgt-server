using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Games.Wishlist;
using Domain;
using Domain.Media;
using Domain.Wishlist;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Persistence;

namespace Core.Test.Games.Wishlist;

[TestClass]
public class GetAllGameWishlistsTest
{
    private static SqliteConnection? Connection { get; set; }
    
    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }
    
    private static DatabaseContext? InMemDatabase { get; set; }

    private static IMapper? Mapper { get; set; }

    private static GetAllGameWishlistsHandler? GetAllGameWishlistsHandler { get; set; }

    private const string FakeUserRemoteId = "d33Z_NuT5";
    private const string FakeDiffUserRemoteId = "d33Z_NuT5+L+M41d3Nl35S";

    [ClassInitialize]
    public static async Task TestClassInit(TestContext context)
    {
        var fakeGameWishlistsList = new List<GameWishlist>()
        {
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                GameRemoteId = 0,
                Platform = "PC"
            },
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                GameRemoteId = 1,
                Platform = "PC"
            },
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                GameRemoteId = 2,
                Platform = "Switch"
            },
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                GameRemoteId = 3,
                Platform = "PC"
            },
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                GameRemoteId = 4,
                Platform = "PC"
            },
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                GameRemoteId = 5,
                Platform = "PS5"
            },
            new()
            {
                UserRemoteId = FakeDiffUserRemoteId,
                GameRemoteId = 6,
                Platform = "PS5"
            }
        };

        var fakeGamesList = new List<Game>()
        {
            new() { RemoteId = 0 },
            new() { RemoteId = 1 },
            new() { RemoteId = 2 },
            new() { RemoteId = 3 },
            new() { RemoteId = 4 },
            new() { RemoteId = 5 },
            new() { RemoteId = 6 },
        };
        
        // Setup in memory database
        Connection = new SqliteConnection("Filename=:memory:");
        Connection.Open();

        ContextOptions = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite(Connection)
            .Options;

        InMemDatabase = new DatabaseContext(ContextOptions);
        await InMemDatabase.Database.EnsureCreatedAsync();
        InMemDatabase.GameWishlists.AddRange(fakeGameWishlistsList);
        InMemDatabase.Games.AddRange(fakeGamesList);
        await InMemDatabase.SaveChangesAsync();

        var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile<MappingProfiles>(); });
        Mapper = mappingConfig.CreateMapper();

        GetAllGameWishlistsHandler = new GetAllGameWishlistsHandler(InMemDatabase);
    }

    [ClassCleanup]
    public static async Task TestClassCleanup()
    {
        await Connection!.DisposeAsync();
    }


    [TestMethod]
    public async Task GetAllGameWishlists_Default()
    {
        // Setup
        var query = new GetAllGameWishlistsQuery()
        {
            UserRemoteId = FakeUserRemoteId,
        };

        // Execute
        var result = await GetAllGameWishlistsHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual(6, result.TotalCount);
    }

    [TestMethod]
    public async Task GetAllGameWishlists_SortByPlatform()
    {
        // Setup
        var query = new GetAllGameWishlistsQuery()
        {
            UserRemoteId = FakeUserRemoteId,
            SortByPlatform = true
        };

        // Execute
        var result = await GetAllGameWishlistsHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual(6, result.TotalCount);
        Assert.AreEqual("PC", result.Items.First().Platform);
        Assert.AreEqual("Switch", result.Items.Last().Platform);
    }
}