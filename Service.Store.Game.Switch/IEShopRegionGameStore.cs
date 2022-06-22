namespace Service.Store.Game.Switch;

public interface IEShopRegionGameStore
{
    public Task<string?> SearchGameStoreId(string region, string gameTitle);
    
    public Task<StoreGamePrice?> GetGamePrice(string region, string gameStoreId);
}