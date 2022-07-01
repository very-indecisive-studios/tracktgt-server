using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Exceptions;
using Core.Users.Register;
using Domain.User;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Persistence;

namespace Core.Test.Users.Register;

[TestClass]
public class RegisterUserTest
{
    private static SqliteConnection? Connection { get; set; }
    
    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }
    
    private static DatabaseContext? InMemDatabase { get; set; }
    
    private static IMapper? Mapper { get; set; }
    
    private static RegisterUserHandler? RegisterUserHandler { get; set; }

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

        RegisterUserHandler = new RegisterUserHandler(InMemDatabase, Mapper);
    }

    [TestMethod]
    public async Task RegisterUser_Default()
    {
        // Setup
        var fakeUserRemoteId = "b0f4d33zn0tz";
        var fakeEmail = "bofa@example.com";
        var fakeUserName = "bofa";
        var query = new RegisterUserCommand(fakeUserRemoteId, fakeEmail, fakeUserName);
        
        // Execute
        await RegisterUserHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.IsTrue(await InMemDatabase!.Users.AnyAsync(u => u.RemoteId == fakeUserRemoteId 
                                                   && u.Email == fakeEmail && u.UserName == fakeUserName));
    }
    
    [TestMethod]
    public async Task RegisterUser_RemoteIdExists()
    {
        // Setup
        var query = new RegisterUserCommand("m4nU_L+rAtIo", "fernandeeznotx@manu.com", "fernandeez");
        
        // Execute
        // Verify
        await Assert.ThrowsExceptionAsync<ExistsException>(() => RegisterUserHandler!.Handle(query, CancellationToken.None));
    }
    
    [TestMethod]
    public async Task RegisterUser_EmailExists()
    {
        // Setup
        var query = new RegisterUserCommand("sPuRXnotrophies?", "christanaldo@manu.com", "balecomeback");
        
        // Execute
        // Verify
        await Assert.ThrowsExceptionAsync<ExistsException>(() => RegisterUserHandler!.Handle(query, CancellationToken.None));
    }
    
    [TestMethod]
    public async Task RegisterUser_UserNameExists()
    {
        // Setup
        var query = new RegisterUserCommand("s0nB3+T3r>sA14h", "heuminson@spurs.com", "christanaldo");
        
        // Execute
        // Verify
        await Assert.ThrowsExceptionAsync<ExistsException>(() => RegisterUserHandler!.Handle(query, CancellationToken.None));
    }
}