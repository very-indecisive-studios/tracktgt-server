using Domain;
using Domain.Pricing;
using Service.Store.Game;
using Service.Store.Game.Switch;

namespace Core.Pricing;

public class GameMall : IGameMall
{
    private readonly Dictionary<GameStoreType, IGameStore> _gameStores = new ();

    public GameMall()
    {
        _gameStores[GameStoreType.Switch] = new SwitchGameStore();
    }

    public IGameStore GetGameStore(GameStoreType gameStoreType) => _gameStores[gameStoreType];
}