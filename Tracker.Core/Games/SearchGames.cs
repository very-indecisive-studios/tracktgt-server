using AutoMapper;
using FluentValidation;
using MediatR;
using Tracker.Service.Game;

namespace Tracker.Core.Games;

public record SearchGamesQuery(string GameTitle) : IRequest<SearchGamesResult>;

public class SearchGamesValidator : AbstractValidator<SearchGamesQuery>
{
    public SearchGamesValidator()
    {
        RuleFor(query => query.GameTitle).NotEmpty();
    }
}

public record SearchGamesResult(List<SearchGamesResult.SearchGamesItemResult> Games)
{
    public record SearchGamesItemResult(long RemoteId, string Title, string CoverImageURL, List<string> Platforms);
}

public static class SearchGamesMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<APIGameBasic, SearchGamesResult.SearchGamesItemResult>()
            .ForCtorParam(
                "RemoteId",
                options => options.MapFrom(apiGame => apiGame.Id));
    }
}

public class SearchGamesHandler : IRequestHandler<SearchGamesQuery, SearchGamesResult>
{
    private readonly IGameService _gameService;
    private readonly IMapper _mapper;

    public SearchGamesHandler(IGameService gameService, IMapper mapper)
    {
        _gameService = gameService;
        _mapper = mapper;
    }
    
    public async Task<SearchGamesResult> Handle(SearchGamesQuery searchGamesQuery, CancellationToken cancellationToken)
    {
        var games = await _gameService.SearchGameByTitle(searchGamesQuery.GameTitle);

        return new SearchGamesResult(games.Select(_mapper.Map<APIGameBasic, SearchGamesResult.SearchGamesItemResult>).ToList());
    }
}
