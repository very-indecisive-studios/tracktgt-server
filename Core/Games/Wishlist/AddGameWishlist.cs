using AutoMapper;
using Core.Exceptions;
using Core.Games.Tracking;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Service.Game;

namespace Core.Games.Wishlist;

public record AddGameWishlistCommand(
    string UserRemoteId,
    long GameRemoteId,
    string Platform
) : IRequest<Unit>;

public class AddGameWishlistValidator : AbstractValidator<AddGameWishlistCommand>
{
    public AddGameWishlistValidator()
    {
        RuleFor(c => c.UserRemoteId).NotEmpty();
        RuleFor(c => c.Platform).NotEmpty();
    }
}

public static class AddGameWishlistMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<AddGameWishlistCommand, GameWishlist>();
    }
}

public class AddGameWishlistHandler : IRequestHandler<AddGameWishlistCommand, Unit>
{
    private readonly DatabaseContext _databaseContext;
    private readonly IGameService _gameService;
    private readonly IMapper _mapper;

    public AddGameWishlistHandler(DatabaseContext databaseContext, IGameService gameService, IMapper mapper)
    {
        _databaseContext = databaseContext;
        _gameService = gameService;
        _mapper = mapper;
    }
    
    public async Task<Unit> Handle(AddGameWishlistCommand command, CancellationToken cancellationToken)
    {
        // Verify user.
        bool isUserExists = await _databaseContext.Users
            .AsNoTracking()
            .Where(u => u.RemoteId == command.UserRemoteId)
            .AnyAsync(cancellationToken);

        if (!isUserExists)
        {
            throw new NotFoundException("User not found!");
        }
        
        // Verify if tracked game already exist.
        bool isGameWishlistExists = await _databaseContext.GameWishlists
            .AsNoTracking()
            .Where(gw => gw.GameRemoteId == command.GameRemoteId 
                         && gw.UserRemoteId.Equals(command.UserRemoteId)
                         && gw.Platform.Equals(command.Platform))
            .AnyAsync(cancellationToken);

        if (isGameWishlistExists)
        {
            throw new ExistsException("Wishlisted game already exists!");
        }
        
        // Verify game id.
        bool isGameExists = await _databaseContext.Games
            .AsNoTracking()
            .Where(g => g.RemoteId == command.GameRemoteId)
            .AnyAsync(cancellationToken);
        // Fetch from external API and store in db if game not cached
        if (!isGameExists)
        {
            APIGame? apiGame = await _gameService.GetGameById(command.GameRemoteId);

            if (apiGame == null)
            {
                throw new NotFoundException("Game not found!");
            }

            Game game = _mapper.Map<APIGame, Game>(apiGame);
            _databaseContext.Games.Add(game);
        }

        var gameWishlist = _mapper.Map<AddGameWishlistCommand, GameWishlist>(command);
        _databaseContext.GameWishlists.Add(gameWishlist);
        
        await _databaseContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}