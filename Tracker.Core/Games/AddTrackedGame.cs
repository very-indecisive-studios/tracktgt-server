using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tracker.Core.Exceptions;
using Tracker.Domain;
using Tracker.Persistence;
using Tracker.Service.Game;

namespace Tracker.Core.Games;

public record AddTrackedGameCommand(
    string UserRemoteId,
    long GameRemoteId,
    float HoursPlayed,
    string Platform,
    TrackedGameFormat Format,
    TrackedGameStatus Status,
    TrackedGameOwnership Ownership
) : IRequest<Unit>;

public class AddTrackedGameValidator : AbstractValidator<AddTrackedGameCommand>
{
    public AddTrackedGameValidator()
    {
        RuleFor(c => c.UserRemoteId).NotEmpty();
        RuleFor(c => c.GameRemoteId).NotEmpty();
        RuleFor(c => c.Platform).NotEmpty();
    }
}

public static class AddTrackedGameMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<AddTrackedGameCommand, TrackedGame>();
        
        // APIGame => Game mapping exists in GetGame use case.
    }
}

public class AddTrackedGameHandler : IRequestHandler<AddTrackedGameCommand, Unit>
{
    private readonly DatabaseContext _dbContext;
    private readonly IGameService _gameService;
    private readonly IMapper _mapper;

    public AddTrackedGameHandler(DatabaseContext dbContext, IGameService gameService, IMapper mapper)
    {
        _dbContext = dbContext;
        _gameService = gameService;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(AddTrackedGameCommand command, CancellationToken cancellationToken)
    {
        // Verify user.
        bool isUserExists = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.RemoteId == command.UserRemoteId)
            .AnyAsync(cancellationToken);

        if (!isUserExists)
        {
            throw new NotFoundException("User not found!");
        }
        
        // Verify if tracked game already exist.
        bool isTrackedGameExists = await _dbContext.TrackedGames
            .AsNoTracking()
            .Where(tg => tg.GameRemoteId == command.GameRemoteId && tg.UserRemoteId == command.UserRemoteId)
            .AnyAsync(cancellationToken);

        if (isTrackedGameExists)
        {
            throw new ExistsException("Tracked game already exists!");
        }
        
        // Verify game id.
        bool isGameExists = await _dbContext.Games
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
            _dbContext.Games.Add(game);
        }

        var trackedGame = _mapper.Map<AddTrackedGameCommand, TrackedGame>(command);
        _dbContext.TrackedGames.Add(trackedGame);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}
