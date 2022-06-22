using Domain.Pricing;
using MediatR;

namespace Core.Pricing.Switch;

public record GetSwitchGameStoreRegionsQuery() : IRequest<GetSwitchGameStoreRegionsResult>;

public record GetSwitchGameStoreRegionsResult(List<string> Regions);


public class GetSwitchGameStoreRegionsHandler : IRequestHandler<GetSwitchGameStoreRegionsQuery, GetSwitchGameStoreRegionsResult>
{
    private readonly IGameMall _gameMall;

    public GetSwitchGameStoreRegionsHandler(IGameMall gameMall)
    {
        _gameMall = gameMall;
    }
    
    public Task<GetSwitchGameStoreRegionsResult> Handle(GetSwitchGameStoreRegionsQuery query,
        CancellationToken cancellationToken)
    {
        var regions = _gameMall.GetGameStore(GameStoreType.Switch).GetSupportedRegions();
        return Task.FromResult(new GetSwitchGameStoreRegionsResult(regions));
    }
}
