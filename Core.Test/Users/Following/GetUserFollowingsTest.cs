using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Exceptions;
using Core.Users.Account;
using Core.Users.Following;
using Domain.User;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Persistence;

namespace Core.Test.Users.Following;

[TestClass]
public class GetUserFollowingsTest
{
    private static SqliteConnection? Connection { get; set; }
    
    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }
    
    private static DatabaseContext? InMemDatabase { get; set; }
    
    private static IMapper? Mapper { get; set; }
    
    private static GetUserFollowingsHandler? GetUserFollowingsHandler { get; set; }

    [ClassInitialize]
    public static async Task TestClassInit(TestContext context)
    {
        var fakeUserList = new List<User>
        {
            new ()
            {
                RemoteId = "User1",
                UserName = "User1"
            },
            new ()
            {
                RemoteId = "User2",
                UserName = "User2"
            },
            new ()
            {
                RemoteId = "User3",
                UserName = "User3"
            }
        };

        var fakeFollowList = new List<Follow>
        {
            new () 
            {
                FollowerUserId = "User1",
                FollowingUserId = "User2"
            },
            new () 
            {
                FollowerUserId = "User1",
                FollowingUserId = "User3"
            },
            new ()
            {
                FollowerUserId = "User2",
                FollowingUserId = "User1"
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
        await InMemDatabase.Users.AddRangeAsync(fakeUserList);
        await InMemDatabase.Follows.AddRangeAsync(fakeFollowList);
        await InMemDatabase.SaveChangesAsync();
        
        var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile<MappingProfiles>(); });
        Mapper = mappingConfig.CreateMapper();

        GetUserFollowingsHandler = new GetUserFollowingsHandler(InMemDatabase, Mapper);
    }

    [TestMethod]
    public async Task GetUserFollowings_Default()
    {
        // Setup
        var query = new GetUserFollowingsQuery("User1");
        
        // Execute
        var result = await GetUserFollowingsHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual(2, result.Items.Count);
        Assert.IsNotNull(result.Items.Find(u => u.UserName.Equals("User2")));
        Assert.IsNotNull(result.Items.Find(u => u.UserName.Equals("User3")));
    }
}