using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Exceptions;
using Core.Games.Wishlist;
using Domain;
using Domain.Wishlist;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Persistence;

namespace Core.Test.Games.Wishlist;

[TestClass]
public class RemoveGameWishlistTest
{
    private static SqliteConnection? Connection { get; set; }

    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }

    private static DatabaseContext? InMemDatabase { get; set; }

    private static IMapper? Mapper { get; set; }

    private static RemoveGameWishlistHandler? RemoveGameWishlistHandler { get; set; }

    private const string FakeUserRemoteId = "d33Z_NuT5";
    private const string FakeGamePlatform = "PC";
    private const long FakeGameRemoteId = 0;
    private const long FakeDoesNotExistGameRemoteId = 999;

    [ClassInitialize]
    public static async Task TestClassInit(TestContext context)
    {
        var fakeGameWishlistsList = new List<GameWishlist>()
        {
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                GameRemoteId = FakeGameRemoteId,
                Platform = "PC",
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

        RemoveGameWishlistHandler = new RemoveGameWishlistHandler(InMemDatabase);
    }

    [TestMethod]
    public async Task RemoveGameWishlist_Exists()
    {
        // Setup
        var command = new RemoveGameWishlistCommand(FakeUserRemoteId, FakeGameRemoteId, FakeGamePlatform);
        
        // Execute
        await RemoveGameWishlistHandler!.Handle(command, CancellationToken.None);

        // Verify
        var count = await InMemDatabase!.GameWishlists
            .Where(gw => gw.UserRemoteId.Equals(FakeUserRemoteId) 
                        && gw.GameRemoteId.Equals(FakeGameRemoteId)
                        && gw.Platform.Equals(FakeGamePlatform))
            .CountAsync();
        Assert.AreEqual(0, count);
    }

    [TestMethod]
    public async Task RemoveBookTracking_NotExists()
    {
        // Setup
        var command = new RemoveGameWishlistCommand(FakeUserRemoteId, FakeDoesNotExistGameRemoteId, FakeGamePlatform);

        // Execute
        // Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => 
            RemoveGameWishlistHandler!.Handle(command, CancellationToken.None));
    }
}