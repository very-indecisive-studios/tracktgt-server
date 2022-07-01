using System.Linq;
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
public class UpdateProfilePicTest
{
    private static SqliteConnection? Connection { get; set; }
    
    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }
    
    private static DatabaseContext? InMemDatabase { get; set; }
    
    private static IMapper? Mapper { get; set; }
    
    private static UpdateProfilePicHandler? UpdateProfilePicHandler { get; set; }

    private static string UserRemoteId = "m4nU_L+rAtIo";
    
    [ClassInitialize]
    public static async Task TestClassInit(TestContext context)
    {
        var fakeUser = new User()
        {
            RemoteId = UserRemoteId,
            Email = "christanaldo@manu.com",
            UserName = "christanaldo",
            Bio = "SUIIIII",
            ProfilePictureURL = "christanaldo.com"
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
        await InMemDatabase.SaveChangesAsync();
        
        var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile<MappingProfiles>(); });
        Mapper = mappingConfig.CreateMapper();

        UpdateProfilePicHandler = new UpdateProfilePicHandler(InMemDatabase, Mapper);
    }

    [TestMethod]
    public async Task UpdateProfilePic_Found()
    {
        // Setup
        var newProfilePic = "pic.messi.com";
        var command = new UpdateProfilePicCommand(UserRemoteId, newProfilePic);
        
        // Execute
        await UpdateProfilePicHandler!.Handle(command, CancellationToken.None);

        // Verify
        var user = await InMemDatabase!.Users
            .AsNoTracking()
            .Where(u => u.RemoteId == "m4nU_L+rAtIo")
            .FirstOrDefaultAsync();
        
        Assert.IsNotNull(user);
        Assert.AreEqual(user.ProfilePictureURL, newProfilePic);
    }
    
    [TestMethod]
    public async Task UpdateProfilePic_NotFound()
    {
        // Setup
        var newProfilePic = "pic.bale.com";
        var nonExistentUserRemoteId = "noexistnomore";
        var command = new UpdateProfilePicCommand(nonExistentUserRemoteId, newProfilePic);
        
        // Execute
        // Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => UpdateProfilePicHandler!.Handle(command, CancellationToken.None));
    }
}