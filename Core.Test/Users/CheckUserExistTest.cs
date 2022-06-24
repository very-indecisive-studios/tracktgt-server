using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Exceptions;
using Core.Users;
using Domain;
using Domain.User;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Persistence;

namespace Core.Test.Users;

[TestClass]
public class CheckUserExistTest
{
    private static SqliteConnection? Connection { get; set; }
    
    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }
    
    private static DatabaseContext? InMemDatabase { get; set; }

    private static CheckUserExistHandler? CheckUserExistHandler { get; set; }

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

        CheckUserExistHandler = new CheckUserExistHandler(InMemDatabase);
    }

    [TestMethod]
    public async Task CheckUserExists_EmailFound()
    {
        // Setup
        var query = new CheckUserExistQuery("gargoyle", "bofa@example.com");
        
        // Execute
        var result = await CheckUserExistHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual(true, result.IsEmailTaken);
        Assert.AreEqual(false, result.IsUserNameTaken);
    }
    
    [TestMethod]
    public async Task CheckUserExists_UsernameFound()
    {
        // Setup
        var query = new CheckUserExistQuery("bofa", "gargoyle@deznotx.com");
        
        // Execute
        var result = await CheckUserExistHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual(false, result.IsEmailTaken);
        Assert.AreEqual(true, result.IsUserNameTaken);
    }
    
    [TestMethod]
    public async Task CheckUserExists_BothFound()
    {
        // Setup
        var query = new CheckUserExistQuery("bofa", "bofa@example.com");
        
        // Execute
        var result = await CheckUserExistHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual(true, result.IsEmailTaken);
        Assert.AreEqual(true, result.IsUserNameTaken);
    }

    
    [TestMethod]
    public async Task CheckUserExists_NotFound()
    {
        // Setup
        var query = new CheckUserExistQuery("gargoyle", "gargoyle@deznotx.com");
        
        // Execute
        var result = await CheckUserExistHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual(false, result.IsEmailTaken);
        Assert.AreEqual(false, result.IsUserNameTaken);
    }
}