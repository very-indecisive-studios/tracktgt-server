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
    string RemoteUserId,
    long RemoteGameId,
    float HoursPlayed,
    string Platform,
    GameFormat Format,
    GameStatus Status,
    GameOwnership Ownership
) : IRequest<Unit>;

public class AddTrackedGameValidator : AbstractValidator<AddTrackedGameCommand>
{
    public AddTrackedGameValidator()
    {
        RuleFor(c => c.RemoteUserId).NotEmpty();
        RuleFor(c => c.RemoteGameId).NotEmpty();
        RuleFor(c => c.Platform).NotEmpty();
    }
}

public static class AddTrackedGameMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<AddTrackedGameCommand, TrackedGame>();
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

    public async Task<Unit> Handle(AddTrackedGameCommand addTrackedGameCommand, CancellationToken cancellationToken)
    {
        User? user = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.RemoteId == addTrackedGameCommand.RemoteUserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User not found!");
        }
        
        // Verify game id
        Game? game = await _dbContext.Games
            .AsNoTracking()
            .Where(g => g.RemoteId == addTrackedGameCommand.RemoteGameId)
            .FirstOrDefaultAsync(cancellationToken);
        // Fetch from external and store in db
        if (game == null)
        {
            APIGame? apiGame = await _gameService.GetGameById(addTrackedGameCommand.RemoteGameId);

            if (apiGame == null)
            {
                throw new NotFoundException("Game not found!");
            }

            game = _mapper.Map<APIGame, Game>(apiGame);
            _dbContext.Games.Add(game);
        }

        var trackedGame = _mapper.Map<AddTrackedGameCommand, TrackedGame>(addTrackedGameCommand);
        _dbContext.TrackedGames.Add(trackedGame);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}
