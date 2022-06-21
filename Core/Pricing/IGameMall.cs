using Domain;
using Domain.Pricing;
using Service.Store.Game;

namespace Core.Pricing;

public interface IGameMall
{
    IGameStore GetGameStore(GameStoreType gameStoreType);
}
