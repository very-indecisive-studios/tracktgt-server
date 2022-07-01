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
public class FollowUserTest
{
    private static SqliteConnection? Connection { get; set; }
    
    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }
    
    private static DatabaseContext? InMemDatabase { get; set; }
    
    private static IMapper? Mapper { get; set; }
    
    private static FollowUserHandler? FollowUserHandler { get; set; }

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
        
        // Setup in memory database
        Connection = new SqliteConnection("Filename=:memory:");
        Connection.Open();

        ContextOptions = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite(Connection)
            .Options;

        InMemDatabase = new DatabaseContext(ContextOptions);
        await InMemDatabase.Database.EnsureCreatedAsync();
        await InMemDatabase.Users.AddRangeAsync(fakeUserList);
        await InMemDatabase.SaveChangesAsync();
        
        var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile<MappingProfiles>(); });
        Mapper = mappingConfig.CreateMapper();

        FollowUserHandler = new FollowUserHandler(InMemDatabase, Mapper);
    }

    [TestMethod]
    public async Task FollowUser_Default()
    {
        // Setup
        var command = new FollowUserCommand("User1", "User2");
        
        // Execute
        var result = await FollowUserHandler!.Handle(command, CancellationToken.None);

        // Verify
        var hasRelation = await InMemDatabase!.Follows
            .Where(f => f.FollowerUserId == "User1" && f.FollowingUserId == "User2")
            .AnyAsync();
        Assert.IsTrue(hasRelation);
    }
    
    [TestMethod]
    public async Task FollowUser_AlreadyExist()
    {
        // Setup
        var command = new FollowUserCommand("User1", "User2");
        
        // Execute
        await Assert.ThrowsExceptionAsync<ExistsException>(() => FollowUserHandler!.Handle(command, CancellationToken.None));
    }
}