using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Books.Wishlist;
using Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Persistence;

namespace Core.Test.Books.Wishlist;

[TestClass]
public class GetBookWishlistTest
{
    private static SqliteConnection? Connection { get; set; }

    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }

    private static DatabaseContext? InMemDatabase { get; set; }

    private static IMapper? Mapper { get; set; }

    private static GetBookWishlistHandler? GetBookWishlistHandler { get; set; }

    private const string FakeUserRemoteId = "d33Z_NuT5";
    private const string FakeBookRemoteId = "0";

    [ClassInitialize]
    public static async Task TestClassInit(TestContext context)
    {
        var fakeBookWishlistsList = new List<BookWishlist>()
        {
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                BookRemoteId = FakeBookRemoteId
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
        InMemDatabase.BookWishlists.AddRange(fakeBookWishlistsList);
        await InMemDatabase.SaveChangesAsync();

        var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile<MappingProfiles>(); });
        Mapper = mappingConfig.CreateMapper();

        GetBookWishlistHandler = new GetBookWishlistHandler(InMemDatabase, Mapper);
    }

    [ClassCleanup]
    public static async Task TestClassCleanup()
    {
        await Connection!.DisposeAsync();
    }

    [TestMethod]
    public async Task GetBookWishlist_Found()
    {
        // Setup
        var query = new GetBookWishlistQuery(FakeUserRemoteId, FakeBookRemoteId);

        // Execute
        var result = await GetBookWishlistHandler!.Handle(query, CancellationToken.None);
        
        // Verify
        Assert.IsTrue(result);
    }
    
    [TestMethod]
    public async Task GetBookWishlist_NotFound()
    {
        // Setup
        var query = new GetBookWishlistQuery(FakeUserRemoteId, "BOOKNOTEXIST");

        // Execute
        var result = await GetBookWishlistHandler!.Handle(query, CancellationToken.None);
        
        // Verify
        Assert.IsFalse(result);
    }
}