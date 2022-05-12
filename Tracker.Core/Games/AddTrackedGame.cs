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
    GameFormat Format,
    GameStatus Status,
    GameOwnership Ownership
) : IRequest<MediatR.Unit>;

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

public class AddTrackedGameHandler : IRequestHandler<AddTrackedGameCommand, MediatR.Unit>
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

    public async Task<MediatR.Unit> Handle(AddTrackedGameCommand addTrackedGameCommand, CancellationToken cancellationToken)
    {
        // Verify user.
        bool isUserExists = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.RemoteId == addTrackedGameCommand.UserRemoteId)
            .AnyAsync(cancellationToken);

        if (!isUserExists)
        {
            throw new NotFoundException("User not found!");
        }
        
        // Verify game id.
        bool isGameExists = await _dbContext.Games
            .AsNoTracking()
            .Where(g => g.RemoteId == addTrackedGameCommand.GameRemoteId)
            .AnyAsync(cancellationToken);
        // Fetch from external API and store in db if game not cached
        if (!isGameExists)
        {
            APIGame? apiGame = await _gameService.GetGameById(addTrackedGameCommand.GameRemoteId);

            if (apiGame == null)
            {
                throw new NotFoundException("Game not found!");
            }

            Game game = _mapper.Map<APIGame, Game>(apiGame);
            _dbContext.Games.Add(game);
        }

        var trackedGame = _mapper.Map<AddTrackedGameCommand, TrackedGame>(addTrackedGameCommand);
        _dbContext.TrackedGames.Add(trackedGame);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return MediatR.Unit.Value;
    }
}
