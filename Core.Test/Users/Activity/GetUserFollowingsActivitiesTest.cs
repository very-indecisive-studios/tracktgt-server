﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Users.Activity;
using Domain.User;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Persistence;

namespace Core.Test.Users.Activity;

[TestClass]
public class GetUserFollowingsActivitiesTest
{
    private static SqliteConnection? Connection { get; set; }
    
    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }
    
    private static DatabaseContext? InMemDatabase { get; set; }
    
    private static IMapper? Mapper { get; set; }
    
    private static GetUserFollowingsActivitiesHandler? GetUserFollowingsActivitiesHandler { get; set; }

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

        var fakeActivitiesList = new List<Domain.Activity>
        {
            new () 
            {
                UserRemoteId = "User1"
            },
            new () 
            {
                UserRemoteId = "User2"
            },
            new () 
            {
                UserRemoteId = "User2"
            },
            new () 
            {
                UserRemoteId = "User3"
            },
            new () 
            {
                UserRemoteId = "User3"
            },            
            new () 
            {
                UserRemoteId = "User3"
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
        await InMemDatabase.Activities.AddRangeAsync(fakeActivitiesList);
        await InMemDatabase.SaveChangesAsync();
        
        var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile<MappingProfiles>(); });
        Mapper = mappingConfig.CreateMapper();

        GetUserFollowingsActivitiesHandler = new GetUserFollowingsActivitiesHandler(InMemDatabase);
    }

    [TestMethod]
    public async Task GetUserFollowingsActivities_Default()
    {
        // Setup
        var query = new GetUserFollowingsActivitiesQuery("User1");
        
        // Execute
        var result = await GetUserFollowingsActivitiesHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual(5, result.Items.Count);
        Assert.AreEqual(2, result.Items.Count(i => i.UserName.Equals("User2")));
        Assert.AreEqual(3, result.Items.Count(i => i.UserName.Equals("User3")));
    }
}