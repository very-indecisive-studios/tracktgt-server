using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Pricing;
using Core.Pricing.Switch;
using Domain.Pricing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Service.Store.Game;

namespace Core.Test.Pricing.Switch;

[TestClass]
public class GetSwitchGameStoreRegionsTest
{
    private static Mock<IGameMall>? MockGameMall { get; set; }

    private static Mock<IGameStore>? MockGameStore { get; set; }
    
    private static GetSwitchGameStoreRegionsHandler? GetSwitchGameStoreRegionsHandler { get; set; }


    [ClassInitialize]
    public static async Task TestClassInit(TestContext context)
    {
        // Setup mocks
        MockGameMall = new Mock<IGameMall>();
        MockGameStore = new Mock<IGameStore>();

        GetSwitchGameStoreRegionsHandler =
            new GetSwitchGameStoreRegionsHandler(MockGameMall.Object);
    }

    [TestCleanup]
    public void TestCaseCleanup()
    {
        MockGameMall.Reset();
        MockGameStore.Reset();
    }
    
    [TestMethod]
    public async Task GetSwitchGameStoreRegions_Default()
    {
        // Setup
        MockGameStore.Setup(gs => gs.GetSupportedRegions())
            .Returns(new List<string> { "AU", "US" });
        MockGameMall!.Setup(gm => gm.GetGameStore(GameStoreType.Switch))
            .Returns(MockGameStore.Object);

        var query = new GetSwitchGameStoreRegionsQuery();

        // Execute
        var result = await GetSwitchGameStoreRegionsHandler!.Handle(query, CancellationToken.None);

        // Verify
        Assert.AreEqual(2, result.Regions.Count);
    }
}