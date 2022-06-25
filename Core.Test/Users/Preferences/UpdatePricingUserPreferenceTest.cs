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
public class UpdatePricingUserPreferenceTest
{
    private static SqliteConnection? Connection { get; set; }

    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }

    private static DatabaseContext? InMemDatabase { get; set; }

    private static IMapper? Mapper { get; set; }

    private static UpdatePricingUserPreferenceHandler? UpdatePricingUserPreferenceHandler { get; set; }

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

        UpdatePricingUserPreferenceHandler = new UpdatePricingUserPreferenceHandler(InMemDatabase, Mapper);
    }

    [TestMethod]
    public async Task UpdatePricingUserPreference_Default()
    {
        // Setup
        var fakeUserId = "1";
        var fakeNewRegion = "NZ";
        var command = new UpdatePricingUserPreferenceCommand(fakeUserId, fakeNewRegion);

        // Execute
        await UpdatePricingUserPreferenceHandler!.Handle(command, CancellationToken.None);

        // Verify
        var pricingUserPreference = await InMemDatabase.PricingUserPreferences
            .AsNoTracking()
            .Where(pup => pup.UserRemoteId == fakeUserId)
            .FirstOrDefaultAsync();
        Assert.IsNotNull(pricingUserPreference);
        Assert.AreEqual(fakeNewRegion, pricingUserPreference.EShopRegion);
    }
}
    
