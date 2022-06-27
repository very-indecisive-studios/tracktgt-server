using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Pricing;
using Core.Pricing.Switch;
using Domain;
using Domain.Media;
using Domain.Pricing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Persistence;
using Service.Game;
using Service.Store.Game;

namespace Core.Test.Pricing.Switch;

[TestClass]
public class GetSwitchGamePriceTest
{
    private static SqliteConnection? Connection { get; set; }

    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }

    private static DatabaseContext? InMemDatabase { get; set; }

    private static IMapper? Mapper { get; set; }

    private static Mock<IGameMall>? MockGameMall { get; set; }

    private static Mock<IGameStore>? MockGameStore { get; set; }

    private static Mock<IGameService>? MockGameService { get; set; }

    private static GetSwitchGamePriceHandler? GetSwitchGamePriceHandler { get; set; }


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

        GetSwitchGamePriceHandler =
            new GetSwitchGamePriceHandler(InMemDatabase, Mapper, MockGameMall.Object, MockGameService.Object);
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
    public async Task GetSwitchGamePrice_GamePriceCached()
    {
        // Setup
        var fakeGameRemoteId = 1;
        var fakeRegion = "SG";
        var fakeGamePriceAmount = 1.5;

        var fakeGamePrice = new GamePrice
        {
            GameRemoteId = fakeGameRemoteId,
            GameStoreType = GameStoreType.Switch,
            Region = fakeRegion,
            URL = "tracktgt.xyz/chaoschef",
            Currency = "SGD",
            Price = fakeGamePriceAmount,
            IsOnSale = true,
            SaleEnd = new DateTime(2022, 10, 14)
        };

        InMemDatabase!.GamePrices.Add(fakeGamePrice);
        await InMemDatabase.SaveChangesAsync();

        var query = new GetSwitchGamePriceQuery(fakeRegion, fakeGameRemoteId);

        // Execute
        var result = await GetSwitchGamePriceHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.IsNotNull(result);
        Assert.AreEqual(fakeGamePriceAmount, result.Price);
    }

    [TestMethod]
    public async Task GetSwitchGamePrice_GamePriceNotCached_GameStoreIdCached()
    {
        // Setup
        var fakeGameRemoteId = 2;
        var fakeGameStoreId = "2";
        var fakeRegion = "SG";

        var fakeGameStoreMetadata = new GameStoreMetadata()
        {
            GameRemoteId = fakeGameRemoteId,
            GameStoreType = GameStoreType.Switch,
            Region = fakeRegion,
            GameStoreId = fakeGameStoreId
        };

        InMemDatabase!.GameStoreMetadatas.Add(fakeGameStoreMetadata);
        await InMemDatabase.SaveChangesAsync();

        var fakeGamePriceAmount = 1.5;
        var fakeStoreGamePrice = new StoreGamePrice
        (
            "tracktgt.xyz/chaoschefultimate",
            "SGD",
            fakeGamePriceAmount,
            true,
            new DateTime(2022, 10, 14)
        );

        MockGameStore!.Setup(gs => gs.GetGamePrice(fakeRegion, fakeGameStoreId))
            .ReturnsAsync(fakeStoreGamePrice);
        MockGameMall!.Setup(gm => gm.GetGameStore(GameStoreType.Switch))
            .Returns(MockGameStore.Object);

        var query = new GetSwitchGamePriceQuery(fakeRegion, fakeGameRemoteId);

        // Execute
        var result = await GetSwitchGamePriceHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.IsNotNull(result);
        Assert.AreEqual(fakeGamePriceAmount, result.Price);

        MockGameStore.Verify(gs => gs.GetGamePrice(fakeRegion, fakeGameStoreId));

        var savedToDB = await InMemDatabase.GamePrices
            .Where(gp => gp.GameRemoteId == fakeGameRemoteId
                         && gp.GameStoreType == GameStoreType.Switch
                         && gp.Region == fakeRegion)
            .AnyAsync();
        Assert.IsTrue(savedToDB);
    }

    [TestMethod]
    public async Task GetSwitchGamePrice_GamePriceNotCached_GameStoreIdNotCached_GameCached()
    {
        // Setup
        var fakeGameRemoteId = 3;
        var fakeGameStoreId = "3";
        var fakeGameTitle = "3";
        var fakeRegion = "SG";

        var fakeGame = new Game
        {
            RemoteId = fakeGameRemoteId,
            Title = fakeGameTitle,
        };

        InMemDatabase!.Games.Add(fakeGame);
        await InMemDatabase.SaveChangesAsync();

        var fakeGamePriceAmount = 1.5;
        var fakeStoreGamePrice = new StoreGamePrice
        (
            "tracktgt.xyz/chaoschefultimatepro",
            "SGD",
            fakeGamePriceAmount,
            true,
            new DateTime(2022, 10, 14)
        );

        MockGameStore!.Setup(gs => gs.SearchGameStoreId(fakeRegion, fakeGameTitle))
            .ReturnsAsync(fakeGameStoreId);
        MockGameStore!.Setup(gs => gs.GetGamePrice(fakeRegion, fakeGameStoreId))
            .ReturnsAsync(fakeStoreGamePrice);
        MockGameMall!.Setup(gm => gm.GetGameStore(GameStoreType.Switch))
            .Returns(MockGameStore.Object);

        var query = new GetSwitchGamePriceQuery(fakeRegion, fakeGameRemoteId);

        // Execute
        var result = await GetSwitchGamePriceHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.IsNotNull(result);
        Assert.AreEqual(fakeGamePriceAmount, result.Price);

        MockGameStore.Verify(gs => gs.GetGamePrice(fakeRegion, fakeGameStoreId));

        var savedToDB = await InMemDatabase.GamePrices
            .Where(gp => gp.GameRemoteId == fakeGameRemoteId
                         && gp.GameStoreType == GameStoreType.Switch
                         && gp.Region == fakeRegion)
            .AnyAsync();
        Assert.IsTrue(savedToDB);
    }

    [TestMethod]
    public async Task GetSwitchGamePrice_GamePriceNotCached_GameStoreIdNotCached_GameNotCached()
    {
        // Setup
        var fakeGameRemoteId = 4;
        var fakeGameStoreId = "4";
        var fakeGameTitle = "4";
        var fakeRegion = "SG";

        var fakeAPIGame = new APIGame
        (
            fakeGameRemoteId, 
            "", 
            fakeGameTitle, 
            "", 
            100, 
            new(), 
            new()
        );

        var fakeGamePriceAmount = 1.5;
        var fakeStoreGamePrice = new StoreGamePrice
        (
            "tracktgt.xyz/chaoschefultimatepro",
            "SGD",
            fakeGamePriceAmount,
            true,
            new DateTime(2022, 10, 14)
        );

        MockGameService!.Setup(gs => gs.GetGameById(fakeGameRemoteId))
            .ReturnsAsync(fakeAPIGame);
        MockGameStore!.Setup(gs => gs.SearchGameStoreId(fakeRegion, fakeGameTitle))
            .ReturnsAsync(fakeGameStoreId);
        MockGameStore!.Setup(gs => gs.GetGamePrice(fakeRegion, fakeGameStoreId))
            .ReturnsAsync(fakeStoreGamePrice);
        MockGameMall!.Setup(gm => gm.GetGameStore(GameStoreType.Switch))
            .Returns(MockGameStore.Object);

        var query = new GetSwitchGamePriceQuery(fakeRegion, fakeGameRemoteId);

        // Execute
        var result = await GetSwitchGamePriceHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.IsNotNull(result);
        Assert.AreEqual(fakeGamePriceAmount, result.Price);

        MockGameStore.Verify(gs => gs.GetGamePrice(fakeRegion, fakeGameStoreId));

        var savedToDB = await InMemDatabase.GamePrices
            .Where(gp => gp.GameRemoteId == fakeGameRemoteId
                         && gp.GameStoreType == GameStoreType.Switch
                         && gp.Region == fakeRegion)
            .AnyAsync();
        Assert.IsTrue(savedToDB);
    }

    [TestMethod]
    public async Task GetSwitchGamePrice_GamePriceNotCached_NotExistGame()
    {
        // Setup
        var fakeGameRemoteId = 5;
        var fakeRegion = "SG";

        MockGameService!.Setup(gs => gs.GetGameById(fakeGameRemoteId))
            .ReturnsAsync((APIGame?) null);
        
        var query = new GetSwitchGamePriceQuery(fakeRegion, fakeGameRemoteId);

        // Execute
        var result = await GetSwitchGamePriceHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.IsNull(result);
    }
    
    [TestMethod]
    public async Task GetSwitchGamePrice_NotExistGameStoreId()
    {
        // Setup
        var fakeGameRemoteId = 6;
        var fakeGameTitle = "6";
        var fakeRegion = "SG";

        var fakeGame = new Game
        {
            RemoteId = fakeGameRemoteId,
            Title = fakeGameTitle,
        };

        InMemDatabase!.Games.Add(fakeGame);
        await InMemDatabase.SaveChangesAsync();

        MockGameStore!.Setup(gs => gs.SearchGameStoreId(fakeRegion, fakeGameTitle))
            .ReturnsAsync((string?) null);
        MockGameMall!.Setup(gm => gm.GetGameStore(GameStoreType.Switch))
            .Returns(MockGameStore.Object);

        var query = new GetSwitchGamePriceQuery(fakeRegion, fakeGameRemoteId);

        // Execute
        var result = await GetSwitchGamePriceHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.IsNull(result);
    }
}