using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Exceptions;
using Core.Users;
using Core.Users.Account;
using Domain;
using Domain.User;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Persistence;

namespace Core.Test.Users;

[TestClass]
public class GetUserTest
{
    private static SqliteConnection? Connection { get; set; }
    
    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }
    
    private static DatabaseContext? InMemDatabase { get; set; }
    
    private static IMapper? Mapper { get; set; }
    
    private static GetUserHandler? GetUserHandler { get; set; }

    [ClassInitialize]
    public static async Task TestClassInit(TestContext context)
    {
        var fakeUserList = new List<User>
        {
            new ()
            {
                RemoteId = "m4nU_L+rAtIo",
                Email = "christanaldo@manu.com",
                UserName = "christanaldo"
            },
            new ()
            {
                RemoteId = "b0f4d33zn0tz",
                Email = "bofa@example.com",
                UserName = "bofa"
            },
            new ()
            {
                RemoteId = "C4nDiCeNu+z",
                Email = "candice@example.com",
                UserName = "candice"
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

        GetUserHandler = new GetUserHandler(InMemDatabase, Mapper);
    }

    [TestMethod]
    public async Task GetUser_Found()
    {
        // Setup
        var query = new GetUserQuery("b0f4d33zn0tz");
        
        // Execute
        var result = await GetUserHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual("bofa", result.UserName);
        Assert.AreEqual("bofa@example.com", result.Email);
    }
    
    [TestMethod]
    public async Task GetUser_NotFound()
    {
        // Setup
        var query = new GetUserQuery("h3lPm33!");
        
        // Execute
        // Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => GetUserHandler!.Handle(query, CancellationToken.None));
    }
}