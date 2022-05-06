using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tracker.Core.Exceptions;
using Tracker.Domain;
using Tracker.Persistence;
using Tracker.Service.Game;

namespace Tracker.Core.Games;

public record GetGameQuery(long GameId) : IRequest<GetGameResult>;

public class GetGameValidator : AbstractValidator<GetGameQuery>
{
    public GetGameValidator()
    {
        RuleFor(query => query.GameId).NotEmpty();
    }
}

public record GetGameResult(
    long Id,
    string? CoverImageURL,
    string? Title,
    string? Summary,
    double Rating,
    List<string> Platforms,
    List<string> Companies
);

public static class GetGameMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<Game, GetGameResult>()
            .ForSourceMember(game => game.Id,
                options => options.DoNotValidate())
            .ForMember(
                result => result.Id,
                options => options.MapFrom(game => game.RemoteId))
            .ForMember(
                result => result.Platforms,
                options => options.MapFrom(game => game.PlatformsString.Split(';', StringSplitOptions.None))
            )
            .ForMember(
                result => result.Companies,
                options => options.MapFrom(game => game.CompaniesString.Split(';', StringSplitOptions.None))
            );
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
        Game? dbGame = await _dbContext.Games
            .AsNoTracking()
            .Where(game => game.RemoteId == getGameQuery.GameId)
            .FirstOrDefaultAsync(cancellationToken);
        if (dbGame != null)
        {
            return _mapper.Map<Game, GetGameResult>(dbGame);
        }
        
        // Find game from remote if not cached.
        APIGame? remoteGame = await _gameService.GetGameById(getGameQuery.GameId);
        if (remoteGame != null)
        {
            Game newDBGame = _mapper.Map<APIGame, Game>(remoteGame);
            _dbContext.Games.Add(newDBGame);
            await _dbContext.SaveChangesAsync(cancellationToken);
          
            return _mapper.Map<Game, GetGameResult>(newDBGame);
        }

        throw new NotFoundException();
    }
}
