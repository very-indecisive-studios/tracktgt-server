using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain;
using Domain.Pricing;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Service.Game;
using Service.Store.Game;

namespace Core.Pricing.Switch;

public record GetSwitchGamePriceQuery(
    string Region,
    long GameRemoteId
) : IRequest<GetSwitchGamePriceResult?>;

public class GetSwitchGamePriceValidator : AbstractValidator<GetSwitchGamePriceQuery>
{
    public GetSwitchGamePriceValidator()
    {
        RuleFor(q => q.Region).NotEmpty();
    }
}

public record GetSwitchGamePriceResult(
    string URL,
    string Currency,
    double Price,
    bool IsOnSale,
    DateTime? SaleEnd
);

public static class GetSwitchGamePriceMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<StoreGamePrice, GamePrice>();
        profile.CreateMap<GamePrice, GetSwitchGamePriceResult>();
    }
}

public class GetSwitchGamePriceHandler : IRequestHandler<GetSwitchGamePriceQuery, GetSwitchGamePriceResult?>
{
    private readonly DatabaseContext _databaseContext;
    private readonly IMapper _mapper;
    private readonly IGameMall _gameMall;
    private readonly IGameService _gameService;

    public GetSwitchGamePriceHandler(
        DatabaseContext databaseContext, 
        IMapper mapper,
        IGameMall gameMall,
        IGameService gameService
    )
    {
        _databaseContext = databaseContext;
        _mapper = mapper;
        _gameMall = gameMall;
        _gameService = gameService;
    }
    
    public async Task<GetSwitchGamePriceResult?> Handle(GetSwitchGamePriceQuery query,
        CancellationToken cancellationToken)
    {
        // Validate query.
        var validator = new GetSwitchGamePriceValidator();
        var validationResult = await validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
        
        // Fetch cached prices.
        var cachedGamePrice = await _databaseContext.GamePrices
            .AsNoTracking()
            .Where(gp => gp.GameRemoteId == query.GameRemoteId 
                         && gp.GameStoreType == GameStoreType.Switch
                         && gp.Region == query.Region)
            .ProjectTo<GetSwitchGamePriceResult>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
        if (cachedGamePrice != null)
        {
            return _mapper.Map<GetSwitchGamePriceResult>(cachedGamePrice);
        }
        
        // Fetch latest prices if no cached price.
        var gameStoreId = await _databaseContext.GameStoreMetadatas
            .AsNoTracking()
            .Where(gsm => gsm.GameRemoteId == query.GameRemoteId 
                         && gsm.GameStoreType == GameStoreType.Switch
                         && gsm.Region == query.Region)
            .Select(gsm => gsm.GameStoreId)
            .FirstOrDefaultAsync(cancellationToken);
        
        // If no cached id, search for game store id.
        if (gameStoreId == null)
        {
            // Get game title => if null, game does not exist.
            var gameTitle = await _databaseContext.Games
                .Where(g => g.RemoteId == query.GameRemoteId)
                .Select(g => g.Title)
                .FirstOrDefaultAsync(cancellationToken);
            if (gameTitle == null)
            {
                var apiGame = await _gameService.GetGameById(query.GameRemoteId);
                if (apiGame == null)
                {
                    return null;
                }

                gameTitle = apiGame.Title;

                _databaseContext.Games.Add(_mapper.Map<Game>(apiGame));
                await _databaseContext.SaveChangesAsync();
            }
            
            // Search for game store id by game title.
            gameStoreId = await _gameMall
                .GetGameStore(GameStoreType.Switch)
                .SearchGameStoreId(query.Region, gameTitle);
            if (gameStoreId == null)
            {
                return null;
            }
            
            // Store the game store metadata if search successful.
            var gameStoreMetadata = new GameStoreMetadata
            {
                GameRemoteId = query.GameRemoteId,
                GameStoreType = GameStoreType.Switch,
                Region = query.Region,
                GameStoreId = gameStoreId
            };
            _databaseContext.GameStoreMetadatas.Add(gameStoreMetadata);
            
            await _databaseContext.SaveChangesAsync(cancellationToken);
        }

        // Fetch latest game price from store.
        var storeGamePrice = await _gameMall
            .GetGameStore(GameStoreType.Switch)
            .GetGamePrice(query.Region, gameStoreId);
        if (storeGamePrice == null)
        {
            return null;
        }
        
        // Save the fetched price to database.
        var newGamePrice = new GamePrice
        {
            GameRemoteId = query.GameRemoteId,
            GameStoreType = GameStoreType.Switch,
            Region = query.Region,
        };
        newGamePrice = _mapper.Map(storeGamePrice, newGamePrice);
        _databaseContext.GamePrices.Add(newGamePrice);
        
        await _databaseContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<GetSwitchGamePriceResult>(newGamePrice);
    }
}