using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Users.Preferences;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Persistence;

namespace Core.Test.Users.Preferences;

[TestClass]
public class GetPricingUserPreferenceTest
{
    private static SqliteConnection? Connection { get; set; }

    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }

    private static DatabaseContext? InMemDatabase { get; set; }

    private static IMapper? Mapper { get; set; }

    private static GetPricingUserPreferenceHandler? GetPricingUserPreferenceHandler { get; set; }

    [ClassInitialize]
    public static async Task TestClassInit(TestContext context)
    {
        // Setup in memory database
        Connection = new SqliteConnection("Filename=:memory:");
        Connection.Open();

        ContextOptions = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite(Connection)
            .Options;

        InMemDatabase = new DatabaseContext(ContextOptions);
        await InMemDatabase.Database.EnsureCreatedAsync();

        var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile<MappingProfiles>(); });
        Mapper = mappingConfig.CreateMapper();

        GetPricingUserPreferenceHandler = new GetPricingUserPreferenceHandler(InMemDatabase, Mapper);
    }

    [TestMethod]
    public async Task GetPricingUserPreference_FirstTimeDefaults()
    {
        // Setup
        var fakeUserId = "1";
        var query = new GetPricingUserPreferenceQuery(fakeUserId);

        // Execute
        var result = await GetPricingUserPreferenceHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual("AU", result.EShopRegion);

        var isExistsInDB = await InMemDatabase.PricingUserPreferences
            .Where(pup => pup.UserRemoteId == fakeUserId)
            .AnyAsync();
        Assert.IsTrue(isExistsInDB);
    }
}
    
