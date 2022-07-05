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
public class CheckUserFollowingTest
{
    private static SqliteConnection? Connection { get; set; }
    
    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }
    
    private static DatabaseContext? InMemDatabase { get; set; }
    
    private static IMapper? Mapper { get; set; }
    
    private static CheckUserFollowingHandler? CheckUserFollowingHandler { get; set; }

    [ClassInitialize]
    public static async Task TestClassInit(TestContext context)
    {
        var fakeUserList = new List<User>
        {
            new ()
            {
                RemoteId = "User1"
            },
            new ()
            {
                RemoteId = "User2"
            },
        };

        var fakeFollow = new Follow()
        {
            FollowerUserId = "User1",
            FollowingUserId = "User2"
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
        InMemDatabase.Follows.Add(fakeFollow);
        await InMemDatabase.SaveChangesAsync();
        
        var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile<MappingProfiles>(); });
        Mapper = mappingConfig.CreateMapper();

        CheckUserFollowingHandler = new CheckUserFollowingHandler(InMemDatabase);
    }

    [TestMethod]
    public async Task CheckUserFollowing_HasFollow()
    {
        // Setup
        var query = new CheckUserFollowingQuery("User1", "User2");
        
        // Execute
        var result = await CheckUserFollowingHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.IsTrue(result.IsFollowing);
    }
    
    [TestMethod]
    public async Task UnfollowUser_NotFollowing()
    {
        var query = new CheckUserFollowingQuery("User2", "User1");
        
        // Execute
        var result = await CheckUserFollowingHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.IsFalse(result.IsFollowing);
    }
}