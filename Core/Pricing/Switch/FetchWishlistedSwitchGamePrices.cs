using AutoMapper;
using Domain;
using Domain.Media;
using Domain.Pricing;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Service.Game;

namespace Core.Pricing.Switch;

public record FetchWishlistedSwitchGamePricesCommand() 
    : IRequest<Unit>;

public class FetchWishlistedSwitchGamePricesHandler : IRequestHandler<FetchWishlistedSwitchGamePricesCommand, Unit>
{
    private readonly DatabaseContext _databaseContext;
    private readonly IGameMall _gameMall;
    private readonly IGameService _gameService;
    private readonly IMapper _mapper;

    public FetchWishlistedSwitchGamePricesHandler(
        DatabaseContext databaseContext, 
        IGameMall gameMall,
        IGameService gameService,
        IMapper mapper
    )
    {
        _databaseContext = databaseContext;
        _gameMall = gameMall;
        _gameService = gameService;
        _mapper = mapper;
    }
    
    public async Task<Unit> Handle(FetchWishlistedSwitchGamePricesCommand command,
        CancellationToken cancellationToken)
    {
        var switchGameStore = _gameMall.GetGameStore(GameStoreType.Switch);
        var switchGameStoreRegions = switchGameStore.GetSupportedRegions();
        
        var wishlistedGamesCount = await _databaseContext.GameWishlists
            .Where(gw => gw.Platform == "Switch")
            .CountAsync(cancellationToken);
        const int maxSize = 10;
        var totalPages = Math.Ceiling(wishlistedGamesCount / (float) maxSize);
        
        for (int currentPage = 1; currentPage <= totalPages; currentPage++)
        {
            var allWishlistedGameRemoteIds = await _databaseContext.GameWishlists
                .Where(gw => gw.Platform == "Switch")
                .Select(gw => gw.GameRemoteId)
                .Distinct()
                .Skip((currentPage - 1) * maxSize)
                .Take(maxSize)
                .ToListAsync(cancellationToken);
            
            foreach (var wishlistedGameRemoteId in allWishlistedGameRemoteIds)
            {
                foreach (var region in switchGameStoreRegions)
                {
                    // Fetch latest prices.
                    var gameStoreId = await _databaseContext.GameStoreMetadatas
                        .AsNoTracking()
                        .Where(gsm => gsm.GameRemoteId == wishlistedGameRemoteId
                                     && gsm.GameStoreType == GameStoreType.Switch
                                     && gsm.Region == region)
                        .Select(gsm => gsm.GameStoreId)
                        .FirstOrDefaultAsync(cancellationToken);
                    
                    // If no cached id, search for game store id.
                    if (gameStoreId == null)
                    {
                        // Get game title => if null, game does not exist.
                        var gameTitle = await _databaseContext.Games
                            .Where(g => g.RemoteId == wishlistedGameRemoteId)
                            .Select(g => g.Title)
                            .FirstOrDefaultAsync(cancellationToken);
                        if (gameTitle == null)
                        {
                            var apiGame = await _gameService.GetGameById(wishlistedGameRemoteId);
                            if (apiGame == null)
                            {
                                continue;
                            }

                            gameTitle = apiGame.Title;

                            _databaseContext.Games.Add(_mapper.Map<Game>(apiGame));
                            await _databaseContext.SaveChangesAsync(cancellationToken);
                        }
                        
                        // Search for game store id by game title.
                        gameStoreId = await _gameMall
                            .GetGameStore(GameStoreType.Switch)
                            .SearchGameStoreId(region, gameTitle);
                        if (gameStoreId == null)
                        {
                            continue;
                        }
                        
                        // Store the game store metadata if search successful.
                        var gameStoreMetadata = new GameStoreMetadata
                        {
                            GameRemoteId = wishlistedGameRemoteId,
                            GameStoreType = GameStoreType.Switch,
                            Region = region,
                            GameStoreId = gameStoreId
                        };
                        _databaseContext.GameStoreMetadatas.Add(gameStoreMetadata);
                        
                        await _databaseContext.SaveChangesAsync(cancellationToken);
                    }

                    // Fetch latest game price from store.
                    var storeGamePrice = await _gameMall
                        .GetGameStore(GameStoreType.Switch)
                        .GetGamePrice(region, gameStoreId);
                    if (storeGamePrice == null)
                    {
                        continue;
                    }
                    
                    // Save or update the fetched price to database.
                    var cachedGamePrice = await _databaseContext.GamePrices
                        .Where(gp => gp.GameRemoteId == wishlistedGameRemoteId 
                                     && gp.GameStoreType == GameStoreType.Switch
                                     && gp.Region == region)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (cachedGamePrice == null)
                    {
                        var newGamePrice = new GamePrice
                        {
                            GameRemoteId = wishlistedGameRemoteId,
                            GameStoreType = GameStoreType.Switch,
                            Region = region,
                        };
                        newGamePrice = _mapper.Map(storeGamePrice, newGamePrice);
                        _databaseContext.GamePrices.Add(newGamePrice);
                    }
                    else
                    {
                        cachedGamePrice = _mapper.Map(storeGamePrice, cachedGamePrice);
                        _databaseContext.GamePrices.Update(cachedGamePrice);
                    }
                    
                    await _databaseContext.SaveChangesAsync(cancellationToken);

                    // Backpressure to prevent spamming external API.
                    await Task.Delay(5000, cancellationToken);
                }
            }
        }
        
        return Unit.Value;
    }
}
