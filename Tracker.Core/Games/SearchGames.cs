using AutoMapper;
using FluentValidation;
using MediatR;
using Tracker.Service.Game;

namespace Tracker.Core.Games;

public class SearchGames
{
    public class Query : IRequest<Result>
    {
        public string GameTitle { get; set; }
    }
    
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(query => query.GameTitle).NotEmpty();
        }
    }

    public class Result
    {
        public class SearchGameResult
        {
            public long Id { get; set; }
    
            public string Title { get; set; }
    
            public List<string> Platforms { get; set; }
        }
        
        public List<SearchGameResult> Games { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result>
    {
        private readonly IGameService _gameService;
        private readonly IMapper _mapper;

        public Handler(IGameService gameService, IMapper mapper)
        {
            _gameService = gameService;
            _mapper = mapper;
        }
        
        public async Task<Result> Handle(Query query, CancellationToken cancellationToken)
        {
            var games = await _gameService.SearchGameByTitle(query.GameTitle);

            return new Result { Games = games.Select(_mapper.Map<APIGame, Result.SearchGameResult>).ToList() };
        }
    }
}
