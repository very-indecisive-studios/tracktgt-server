using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tracker.Core.Exceptions;
using Tracker.Domain;
using Tracker.Persistence;
using Tracker.Service.Game;

namespace Tracker.Core.Games;

public class AddTrackedGame
{
    public class Command : IRequest<Unit>
    {
        public Guid UserId { get; set; }
        
        public long GameId { get; set; }
        
        public float HoursPlayed { get; set; }
        
        public string Platform { get; set; }
        
        public GameFormat Format { get; set; }
        
        public GameStatus Status { get; set; }
        
        public GameOwnership Ownership { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.UserId).NotEmpty();
            RuleFor(c => c.GameId).NotEmpty();
            RuleFor(c => c.Platform).NotEmpty();
        }
    }

    public class Handler : IRequestHandler<Command, Unit>
    {
        private readonly DatabaseContext _dbContext;
        private readonly IGameService _gameService;
        private readonly IMapper _mapper;

        public Handler(DatabaseContext dbContext, IGameService gameService, IMapper mapper)
        {
            _dbContext = dbContext;
            _gameService = gameService;
            _mapper = mapper;
        }

        public async Task<Unit> Handle(Command command, CancellationToken cancellationToken)
        {
            // TODO: Verify user id
            
            
            // Verify game id
            Game? game = await _dbContext.Games
                .AsNoTracking()
                .Where(g => g.RemoteId == command.GameId)
                .FirstOrDefaultAsync(cancellationToken);
            // Fetch from external and store in db
            if (game == null)
            {
                APIGame? apiGame = await _gameService.GetGameById(command.GameId);

                if (apiGame == null)
                {
                    throw new NotFoundException("Game not found!");
                }

                game = _mapper.Map<APIGame, Game>(apiGame);
                _dbContext.Games.Add(game);
            }

            var trackedGame = new TrackedGame();
            _mapper.Map<Command, TrackedGame>(command, trackedGame);
            _dbContext.TrackedGames.Add(trackedGame);
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return Unit.Value;
        }
    }
}

