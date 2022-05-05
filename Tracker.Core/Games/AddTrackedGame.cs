using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tracker.Core.Exceptions;
using Tracker.Domain;
using Tracker.Persistence;
using Tracker.Service.Game;

namespace Tracker.Core.Games;

public class AddTrackedGameCommand : IRequest<Unit>
{
    public string RemoteUserId { get; set; }
    
    public long GameId { get; set; }
    
    public float HoursPlayed { get; set; }
    
    public string Platform { get; set; }
    
    public GameFormat Format { get; set; }
    
    public GameStatus Status { get; set; }
    
    public GameOwnership Ownership { get; set; }
}

public class AddTrackedGameValidator : AbstractValidator<AddTrackedGameCommand>
{
    public AddTrackedGameValidator()
    {
        RuleFor(c => c.RemoteUserId).NotEmpty();
        RuleFor(c => c.GameId).NotEmpty();
        RuleFor(c => c.Platform).NotEmpty();
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
            .Where(g => g.RemoteId == addTrackedGameCommand.GameId)
            .FirstOrDefaultAsync(cancellationToken);
        // Fetch from external and store in db
        if (game == null)
        {
            APIGame? apiGame = await _gameService.GetGameById(addTrackedGameCommand.GameId);

            if (apiGame == null)
            {
                throw new NotFoundException("Game not found!");
            }

            game = _mapper.Map<APIGame, Game>(apiGame);
            _dbContext.Games.Add(game);
        }

        var trackedGame = new TrackedGame()
        {
            UserId = user.Id
        };
        _mapper.Map<AddTrackedGameCommand, TrackedGame>(addTrackedGameCommand, trackedGame);
        _dbContext.TrackedGames.Add(trackedGame);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}
