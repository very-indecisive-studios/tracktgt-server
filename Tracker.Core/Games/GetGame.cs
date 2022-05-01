using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tracker.Core.Exceptions;
using Tracker.Domain;
using Tracker.Persistence;
using Tracker.Service.Game;

namespace Tracker.Core.Games;

public class GetGame
{
    public class Query : IRequest<Result>
    {
        public long GameId { get; set; }
    }

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(query => query.GameId).NotEmpty();
        }
    }

    public class Result
    {
        public long Id { get; set; }
        
        public string Title { get; set; }
        
        public List<string> Platforms { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result>
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
        
        public async Task<Result> Handle(Query query, CancellationToken cancellationToken)
        {
            // Find game from database (cached locally).
            Game? dbGame = await _dbContext.Games
                .AsNoTracking()
                .Where(game => game.RemoteId == query.GameId)
                .FirstOrDefaultAsync(cancellationToken);
            if (dbGame != null)
            {
                return _mapper.Map<Game, Result>(dbGame);
            }
            
            // Find game from remote if not cached.
            APIGame? remoteGame = await _gameService.GetGameById(query.GameId);
            if (remoteGame != null)
            {
                Game newDBGame = _mapper.Map<APIGame, Game>(remoteGame);
                _dbContext.Games.Add(newDBGame);
                await _dbContext.SaveChangesAsync(cancellationToken);
              
                return _mapper.Map<Game, Result>(newDBGame);
            }
    
            throw new NotFoundException();
        }
    }
}