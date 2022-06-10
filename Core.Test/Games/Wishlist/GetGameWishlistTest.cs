using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Games.Wishlist;
using Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Persistence;

namespace Core.Test.Games.Wishlist;

[TestClass]
public class GetGameWishlistTest
{
    private static SqliteConnection? Connection { get; set; }

    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }

    private static DatabaseContext? InMemDatabase { get; set; }

    private static IMapper? Mapper { get; set; }

    private static GetGameWishlistsHandler? GetGameWishlistsHandler { get; set; }

    private const string FakeUserRemoteId = "d33Z_NuT5";
    private const long FakeGameRemoteId = 0;
    private const long FakeDoesNotExistGameRemoteId = 9999;

    [ClassInitialize]
    public static async Task TestClassInit(TestContext context)
    {
        var fakeGameWishlistsList = new List<GameWishlist>()
        {
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                GameRemoteId = FakeGameRemoteId,
                Platform = "PC"
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
        InMemDatabase.GameWishlists.AddRange(fakeGameWishlistsList);
        await InMemDatabase.SaveChangesAsync();

        var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile<MappingProfiles>(); });
        Mapper = mappingConfig.CreateMapper();

        GetGameWishlistsHandler = new GetGameWishlistsHandler(InMemDatabase, Mapper);
    }

    [ClassCleanup]
    public static async Task TestClassCleanup()
    {
        await Connection!.DisposeAsync();
    }

    [TestMethod]
    public async Task GetGameWishlists_Found()
    {
        // Setup
        var query = new GetGameWishlistsQuery(FakeUserRemoteId, FakeGameRemoteId);

        // Execute
        var result = await GetGameWishlistsHandler!.Handle(query, CancellationToken.None);
        
        // Verify
        Assert.IsNotNull(result);
    }
    
    [TestMethod]
    public async Task GetGameWishlists_NotFound()
    {
        // Setup
        var query = new GetGameWishlistsQuery(FakeUserRemoteId, FakeDoesNotExistGameRemoteId);

        // Execute
        var result = await GetGameWishlistsHandler!.Handle(query, CancellationToken.None);
        
        // Verify
        Assert.AreEqual(0, result.Items.Count);
    }
}