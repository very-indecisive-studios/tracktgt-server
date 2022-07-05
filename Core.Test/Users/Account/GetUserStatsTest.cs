using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Exceptions;
using Core.Users.Account;
using Domain.Tracking;
using Domain.User;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Persistence;

namespace Core.Test.Users.Account;

[TestClass]
public class GetUserStatsTest
{
    private static SqliteConnection? Connection { get; set; }
    
    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }
    
    private static DatabaseContext? InMemDatabase { get; set; }
    
    private static IMapper? Mapper { get; set; }
    
    private static GetUserStatsHandler? GetUserStatsHandler { get; set; }

    [ClassInitialize]
    public static async Task TestClassInit(TestContext context)
    {
        var fakeUser= new User()
        {
            RemoteId = "m4nU_L+rAtIo",
            Email = "christanaldo@manu.com",
            UserName = "christanaldo",
            Bio = "SUIIIII",
            ProfilePictureURL = "christanaldo.com"
        };

        var fakeGameTrackingList = new List<GameTracking>
        {
            new () 
            {
                UserRemoteId = "m4nU_L+rAtIo",
                HoursPlayed = 101
            },            
            new () 
            {
                UserRemoteId = "m4nU_L+rAtIo",
                HoursPlayed = 202
            },
        };
        
        var fakeBookTrackingList = new List<BookTracking>
        {
            new () 
            {
                UserRemoteId = "m4nU_L+rAtIo",
                ChaptersRead = 303
            },            
            new () 
            {
                UserRemoteId = "m4nU_L+rAtIo",
                ChaptersRead = 404
            },
        };
        
        var fakeShowTrackingList = new List<ShowTracking>
        {
            new () 
            {
                UserRemoteId = "m4nU_L+rAtIo",
                EpisodesWatched = 505
            },            
            new () 
            {
                UserRemoteId = "m4nU_L+rAtIo",
                EpisodesWatched = 606
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
        InMemDatabase.Users.Add(fakeUser);
        await InMemDatabase.GameTrackings.AddRangeAsync(fakeGameTrackingList);
        await InMemDatabase.BookTrackings.AddRangeAsync(fakeBookTrackingList);
        await InMemDatabase.ShowTrackings.AddRangeAsync(fakeShowTrackingList);
        await InMemDatabase.SaveChangesAsync();
        
        var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile<MappingProfiles>(); });
        Mapper = mappingConfig.CreateMapper();

        GetUserStatsHandler = new GetUserStatsHandler(InMemDatabase, Mapper);
    }

    [TestMethod]
    public async Task GetUserStats_Default()
    {
        // Setup
        var query = new GetUserStatsQuery("m4nU_L+rAtIo");
        
        // Execute
        var result = await GetUserStatsHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual(303, result.GamingHours);
        Assert.AreEqual(707, result.ChaptersRead);
        Assert.AreEqual(1111, result.EpisodesWatched);
    }
}