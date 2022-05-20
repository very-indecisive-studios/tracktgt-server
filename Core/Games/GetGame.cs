using AutoMapper;
using Core.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Domain;
using Persistence;
using Service.Game;

namespace Core.Games;

public record GetGameQuery(long RemoteId) : IRequest<GetGameResult>;

public class GetGameValidator : AbstractValidator<GetGameQuery>
{
    public GetGameValidator()
    {
        RuleFor(query => query.RemoteId).NotEmpty();
    }
}

public record GetGameResult(
    long RemoteId,
    string CoverImageURL,
    string Title,
    string Summary,
    double Rating,
    List<string>? Platforms,
    List<string>? Companies
);

public static class GetGameMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<APIGame, Game>()
            .ForMember(game => game.Id,
                options => options.Ignore())
            .ForMember(
                game => game.RemoteId,
                options => options.MapFrom(apiGame => apiGame.Id))
            .ForMember(
                game => game.PlatformsString,
                options => options.MapFrom(apiGame => string.Join(";", apiGame.Platforms)))
            .ForMember(
                game => game.CompaniesString,
                options => options.MapFrom(apiGame => string.Join(";", apiGame.Companies)));

        profile.CreateMap<Game, GetGameResult>()
            .ForCtorParam(
                "Platforms",
                options => options.MapFrom(game => game.PlatformsString.Split(';', StringSplitOptions.None)))
            .ForCtorParam(
                "Companies",
                options => options.MapFrom(game => game.CompaniesString.Split(';', StringSplitOptions.None)));
    }
}

public class GetGameHandler : IRequestHandler<GetGameQuery, GetGameResult>
{
    private readonly DatabaseContext _dbContext;
    private readonly IGameService _gameService;
    private readonly IMapper _mapper;

    public GetGameHandler(DatabaseContext dbContext, IGameService gameService, IMapper mapper)
    {
        _dbContext = dbContext;
        _gameService = gameService;
        _mapper = mapper;
    }

    public async Task<GetGameResult> Handle(GetGameQuery getGameQuery, CancellationToken cancellationToken)
    {
        // Find game from database (cached locally).
        var dbGame = await _dbContext.Games
            .AsNoTracking()
            .Where(game => game.RemoteId == getGameQuery.RemoteId)
            .FirstOrDefaultAsync(cancellationToken);
        if (dbGame != null) return _mapper.Map<Game, GetGameResult>(dbGame);

        // Find game from remote if not cached.
        var remoteGame = await _gameService.GetGameById(getGameQuery.RemoteId);
        if (remoteGame != null)
        {
            var newDBGame = _mapper.Map<APIGame, Game>(remoteGame);
            _dbContext.Games.Add(newDBGame);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return _mapper.Map<Game, GetGameResult>(newDBGame);
        }

        throw new NotFoundException();
    }
}