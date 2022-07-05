using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Users.Following;
using Domain.User;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Persistence;

namespace Core.Test.Users.Following;

[TestClass]
public class GetTopUsersTest
{
    private static SqliteConnection? Connection { get; set; }
    
    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }
    
    private static DatabaseContext? InMemDatabase { get; set; }
    
    private static IMapper? Mapper { get; set; }
    
    private static GetTopUsersHandler? GetTopUsersHandler { get; set; }

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
            },
            new ()
            {
                RemoteId = "User4",
                UserName = "User4"
            }
        };
    
        // Popularity ranking: 2, 1, 3
        var fakeFollowList = new List<Follow>
        {
            new () 
            {
                FollowerUserId = "User1",
                FollowingUserId = "User2"
            },
            new ()
            {
                FollowerUserId = "User3",
                FollowingUserId = "User2"
            },
            new ()
            {
                FollowerUserId = "User4",
                FollowingUserId = "User2"
            },
            new ()
            {
                FollowerUserId = "User2",
                FollowingUserId = "User1"
            },            
            new ()
            {
                FollowerUserId = "User4",
                FollowingUserId = "User1"
            },
            new () 
            {
                FollowerUserId = "User1",
                FollowingUserId = "User3"
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

        GetTopUsersHandler = new GetTopUsersHandler(InMemDatabase);
    }

    [TestMethod]
    public async Task GetTopUsers_Top3()
    {
        // Setup
        var query = new GetTopUsersQuery(3);
        
        // Execute
        var result = await GetTopUsersHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual("User2", result.Items.ElementAt(0).RemoteId);
        Assert.AreEqual("User1", result.Items.ElementAt(1).RemoteId);
        Assert.AreEqual("User3", result.Items.ElementAt(2).RemoteId);
    }
}