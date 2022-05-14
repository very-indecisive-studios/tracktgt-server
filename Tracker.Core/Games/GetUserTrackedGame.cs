using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tracker.Core.Exceptions;
using Tracker.Domain;
using Tracker.Persistence;

namespace Tracker.Core.Games;

public record GetTrackedGameQuery(
    string UserRemoteId,
    long GameRemoteId
) : IRequest<GetTrackedGameResult>;

public class GetTrackedGameValidator : AbstractValidator<GetTrackedGameQuery>
{
    public GetTrackedGameValidator()
    {
        RuleFor(q => q.UserRemoteId).NotEmpty();
    }    
}

public record GetTrackedGameResult(
    float HoursPlayed,
    string Platform,
    TrackedGameFormat Format,
    TrackedGameStatus Status,
    TrackedGameOwnership Ownership
);

public static class GetTrackedGameMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<TrackedGame, GetTrackedGameResult>();
    }
}

public class GetTrackedGameHandler : IRequestHandler<GetTrackedGameQuery, GetTrackedGameResult>
{
    private readonly DatabaseContext _databaseContext;
    private readonly IMapper _mapper;

    public GetTrackedGameHandler(DatabaseContext databaseContext, IMapper mapper)
    {
        _databaseContext = databaseContext;
        _mapper = mapper;
    }
    
    public async Task<GetTrackedGameResult> Handle(GetTrackedGameQuery query, CancellationToken cancellationToken)
    {
        var trackedGame = await _databaseContext.TrackedGames
            .AsNoTracking()
            .Where(tg => tg.UserRemoteId == query.UserRemoteId && tg.GameRemoteId == query.GameRemoteId)
            .FirstOrDefaultAsync(cancellationToken);

        if (trackedGame == null)
        {
            throw new NotFoundException();
        }

        return _mapper.Map<TrackedGame, GetTrackedGameResult>(trackedGame);
    }
}