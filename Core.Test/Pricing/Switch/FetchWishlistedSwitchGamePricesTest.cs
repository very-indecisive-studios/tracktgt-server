using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Pricing;
using Core.Pricing.Switch;
using Domain;
using Domain.Pricing;
using Domain.Wishlist;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Persistence;
using Service.Game;
using Service.Store.Game;

namespace Core.Test.Pricing.Switch;

[TestClass]
public class FetchWishlistedSwitchGamePricesTest
{
    private static SqliteConnection? Connection { get; set; }

    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }

    private static DatabaseContext? InMemDatabase { get; set; }

    private static IMapper? Mapper { get; set; }

    private static Mock<IGameMall>? MockGameMall { get; set; }

    private static Mock<IGameStore>? MockGameStore { get; set; }

    private static Mock<IGameService>? MockGameService { get; set; }

    private static FetchWishlistedSwitchGamePricesHandler? FetchWishlistedSwitchGamePricesHandler { get; set; }


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

        // Setup mapper.
        var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile<MappingProfiles>(); });
        Mapper = mappingConfig.CreateMapper();

        // Setup mocks
        MockGameMall = new Mock<IGameMall>();
        MockGameStore = new Mock<IGameStore>();
        MockGameService = new Mock<IGameService>();

        FetchWishlistedSwitchGamePricesHandler =
            new FetchWishlistedSwitchGamePricesHandler(InMemDatabase, MockGameMall.Object, MockGameService.Object, Mapper);
    }

    [TestCleanup]
    public void TestCaseCleanup()
    {
        MockGameMall.Reset();
        MockGameStore.Reset();
        MockGameService.Reset();
    }

    [ClassCleanup]
    public static async Task TestClassCleanup()
    {
        await Connection!.DisposeAsync();
    }
    
    [TestMethod]
    public async Task FetchWishlistedSwitchGamePrices_Default()
    {
        // Setup
        var fakeRegion1 = "SG";
        var fakeRegion2 = "AU";
        
        /* Game 1 */
        var fakeGame1RemoteId = 1;
        var fakeGame1StoreId = "1";
        var fakeGame1Wishlist = new GameWishlist
        {
            UserRemoteId = null,
            GameRemoteId = fakeGame1RemoteId,
            Platform = "Switch"
        };
        var fakeGame1StoreMetadata1 = new GameStoreMetadata()
        {
            GameRemoteId = fakeGame1RemoteId,
            GameStoreType = GameStoreType.Switch,
            Region = fakeRegion1,
            GameStoreId = fakeGame1StoreId
        };
        var fakeGame1StoreMetadata2 = new GameStoreMetadata()
        {
            GameRemoteId = fakeGame1RemoteId,
            GameStoreType = GameStoreType.Switch,
            Region = fakeRegion2,
            GameStoreId = fakeGame1StoreId
        };
        var fakeStoreGame1Price = new StoreGamePrice
        (
            "tracktgt.xyz/chaoschefultimate",
            "SGD",
            1,
            true,
            new DateTime(2022, 10, 14)
        );
        
        /* Game 2 */
        var fakeGame2RemoteId = 2;
        var fakeGame2StoreId = "2";
        var fakeGame2Wishlist = new GameWishlist
        {
            UserRemoteId = null,
            GameRemoteId = fakeGame2RemoteId,
            Platform = "Switch"
        };
        var fakeGame2StoreMetadata1 = new GameStoreMetadata()
        {
            GameRemoteId = fakeGame2RemoteId,
            GameStoreType = GameStoreType.Switch,
            Region = fakeRegion1,
            GameStoreId = fakeGame2StoreId
        };
        var fakeGame2StoreMetadata2 = new GameStoreMetadata()
        {
            GameRemoteId = fakeGame2RemoteId,
            GameStoreType = GameStoreType.Switch,
            Region = fakeRegion2,
            GameStoreId = fakeGame2StoreId
        };
        var fakeStoreGame2Price = new StoreGamePrice
        (
            "tracktgt.xyz/chaoschefultimate",
            "SGD",
            2,
            true,
            new DateTime(2022, 10, 14)
        );

        InMemDatabase!.GameWishlists.AddRange(fakeGame1Wishlist, fakeGame2Wishlist);
        InMemDatabase!.GameStoreMetadatas.AddRange
        (
            fakeGame1StoreMetadata1, 
            fakeGame1StoreMetadata2, 
            fakeGame2StoreMetadata1,
            fakeGame2StoreMetadata2
        );
        await InMemDatabase.SaveChangesAsync();

        MockGameStore!.Setup(gs => gs.GetGamePrice(It.IsAny<string>(), fakeGame1StoreId))
            .ReturnsAsync(fakeStoreGame1Price);
        MockGameStore.Setup(gs => gs.GetGamePrice(It.IsAny<string>(), fakeGame2StoreId))
            .ReturnsAsync(fakeStoreGame2Price);
        MockGameStore.Setup(gs => gs.GetSupportedRegions())
            .Returns(new List<string> { fakeRegion1, fakeRegion2 });
        MockGameMall!.Setup(gm => gm.GetGameStore(GameStoreType.Switch))
            .Returns(MockGameStore.Object);

        var command = new FetchWishlistedSwitchGamePricesCommand();

        // Execute
        await FetchWishlistedSwitchGamePricesHandler!.Handle(command, CancellationToken.None);

        // Verify
        var noOfGame1Prices = await InMemDatabase.GamePrices
            .Where(gp => gp.GameRemoteId == fakeGame1RemoteId
                         && gp.GameStoreType == GameStoreType.Switch)
            .CountAsync();
        var noOfGame2Prices = await InMemDatabase.GamePrices
            .Where(gp => gp.GameRemoteId == fakeGame2RemoteId
                         && gp.GameStoreType == GameStoreType.Switch)
            .CountAsync();
        Assert.AreEqual(2, noOfGame1Prices);
        Assert.AreEqual(2, noOfGame2Prices);
    }
}