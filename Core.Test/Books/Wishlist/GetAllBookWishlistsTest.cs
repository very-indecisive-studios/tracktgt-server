using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Books.Wishlist;
using Domain;
using Domain.Media;
using Domain.Wishlist;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Persistence;

namespace Core.Test.Books.Wishlist;

[TestClass]
public class GetAllBookWishlistsTest
{
    private static SqliteConnection? Connection { get; set; }
    
    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }
    
    private static DatabaseContext? InMemDatabase { get; set; }

    private static IMapper? Mapper { get; set; }

    private static GetAllBookWishlistsHandler? GetAllBookWishlistsHandler { get; set; }

    private const string FakeUserRemoteId = "d33Z_NuT5";
    private const string FakeDiffUserRemoteId = "d33Z_NuT5+L+M41d3Nl35S";

    [ClassInitialize]
    public static async Task TestClassInit(TestContext context)
    {
        var fakeBookWishlistsList = new List<BookWishlist>()
        {
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                BookRemoteId = "0"
            },
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                BookRemoteId = "1"
            },
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                BookRemoteId = "2"
            },
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                BookRemoteId = "3"
            },
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                BookRemoteId = "4"
            },
            new()
            {
                UserRemoteId = FakeUserRemoteId,
                BookRemoteId = "5"
            },
            new()
            {
                UserRemoteId = FakeDiffUserRemoteId,
                BookRemoteId = "6"
            }
        };

        var fakeBooksList = new List<Book>()
        {
            new() { RemoteId = "0" },
            new() { RemoteId = "1" },
            new() { RemoteId = "2" },
            new() { RemoteId = "3" },
            new() { RemoteId = "4" },
            new() { RemoteId = "5" },
            new() { RemoteId = "6" },
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
        InMemDatabase.Books.AddRange(fakeBooksList);
        await InMemDatabase.SaveChangesAsync();

        var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile<MappingProfiles>(); });
        Mapper = mappingConfig.CreateMapper();

        GetAllBookWishlistsHandler = new GetAllBookWishlistsHandler(InMemDatabase);
    }

    [ClassCleanup]
    public static async Task TestClassCleanup()
    {
        await Connection!.DisposeAsync();
    }


    [TestMethod]
    public async Task GetAllBookWishlists_Default()
    {
        // Setup
        var query = new GetAllBookWishlistsQuery()
        {
            UserRemoteId = FakeUserRemoteId,
        };

        // Execute
        var result = await GetAllBookWishlistsHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual(6, result.TotalCount);
    }
}