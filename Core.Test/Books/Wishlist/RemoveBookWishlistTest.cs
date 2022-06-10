using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Books.Wishlist;
using Core.Exceptions;
using Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Persistence;

namespace Core.Test.Books.Wishlist;

[TestClass]
public class RemoveBookWishlistTest
{
    private static SqliteConnection? Connection { get; set; }

    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }

    private static DatabaseContext? InMemDatabase { get; set; }

    private static IMapper? Mapper { get; set; }

    private static RemoveBookWishlistHandler? RemoveBookWishlistHandler { get; set; }

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

        RemoveBookWishlistHandler = new RemoveBookWishlistHandler(InMemDatabase);
    }

    [TestMethod]
    public async Task RemoveBookWishlist_Exists()
    {
        // Setup
        var command = new RemoveBookWishlistCommand(FakeUserRemoteId, FakeBookRemoteId);
        
        // Execute
        await RemoveBookWishlistHandler!.Handle(command, CancellationToken.None);

        // Verify
        var count = await InMemDatabase!.BookWishlists
            .Where(b => b.UserRemoteId.Equals(FakeUserRemoteId) 
                        && b.BookRemoteId.Equals(FakeBookRemoteId))
            .CountAsync();
        Assert.AreEqual(0, count);
    }

    [TestMethod]
    public async Task RemoveBookWishlist_NotExists()
    {
        // Setup
        var command = new RemoveBookWishlistCommand(FakeUserRemoteId, FakeBookRemoteId);

        // Execute
        // Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => 
            RemoveBookWishlistHandler!.Handle(command, CancellationToken.None));
    }
}