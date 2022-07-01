using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Exceptions;
using Core.Users.Account;
using Domain.User;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Persistence;

namespace Core.Test.Users.Account;

[TestClass]
public class GetUserByUserNameTest
{
    private static SqliteConnection? Connection { get; set; }
    
    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }
    
    private static DatabaseContext? InMemDatabase { get; set; }
    
    private static IMapper? Mapper { get; set; }
    
    private static GetUserByUserNameHandler? GetUserByUserNameHandler { get; set; }

    [ClassInitialize]
    public static async Task TestClassInit(TestContext context)
    {
        var fakeUserList = new List<User>
        {
            new ()
            {
                RemoteId = "m4nU_L+rAtIo",
                Email = "christanaldo@manu.com",
                UserName = "christanaldo",
                Bio = "SUIIIII",
                ProfilePictureURL = "christanaldo.com"
            },
            new ()
            {
                RemoteId = "b0f4d33zn0tz",
                Email = "bofa@example.com",
                UserName = "bofa",
                Bio = "BOFADEEEEEEEZ",
                ProfilePictureURL = "sheesh.com"
            },
            new ()
            {
                RemoteId = "C4nDiCeNu+z",
                Email = "candice@example.com",
                UserName = "candice",
                Bio = "CANNNDEEEEEEZZZZ",
                ProfilePictureURL = "sheesh1.com"
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
        await InMemDatabase.SaveChangesAsync();
        
        var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile<MappingProfiles>(); });
        Mapper = mappingConfig.CreateMapper();

        GetUserByUserNameHandler = new GetUserByUserNameHandler(InMemDatabase, Mapper);
    }

    [TestMethod]
    public async Task GetUserByUserName_Found()
    {
        // Setup
        var query = new GetUserByUserNameQuery("bofa");
        
        // Execute
        var result = await GetUserByUserNameHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual("b0f4d33zn0tz", result.RemoteId);
        Assert.AreEqual("bofa@example.com", result.Email);
        Assert.AreEqual("BOFADEEEEEEEZ", result.Bio);
        Assert.AreEqual("sheesh.com", result.ProfilePictureURL);
    }
    
    [TestMethod]
    public async Task GetUserByUserName_NotFound()
    {
        // Setup
        var query = new GetUserByUserNameQuery("h3lPm33");
        
        // Execute
        // Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => GetUserByUserNameHandler!.Handle(query, CancellationToken.None));
    }
}