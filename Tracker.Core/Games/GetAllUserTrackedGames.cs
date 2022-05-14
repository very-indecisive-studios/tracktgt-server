using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tracker.Core.Common;
using Tracker.Domain;
using Tracker.Persistence;

namespace Tracker.Core.Games;

public class GetAllUserTrackedGamesQuery : PagedListRequest, IRequest<PagedListResult<GetAllUserTrackedGamesItemResult>>
{
    public GetAllUserTrackedGamesQuery(string userRemoteId)
    {
        UserRemoteId = userRemoteId;
    }

    public string UserRemoteId { get; }
    
    public TrackedGameStatus? GameStatus { get; init; } = null;
    
    public bool SortByHoursPlayed { get; init; } = false;
    
    public bool SortByPlatform { get; init; } = false;
    
    public bool SortByFormat { get; init; } = false;
    
    public bool SortByOwnership { get; init; } = false;
}

public class GetAllUserTrackedGamesValidator : AbstractValidator<GetAllUserTrackedGamesQuery>
{
    public GetAllUserTrackedGamesValidator()
    {
        RuleFor(q => q.UserRemoteId).NotEmpty();
    }    
}

public record GetAllUserTrackedGamesItemResult(
    long GameRemoteId,
    float HoursPlayed,
    string Platform,
    TrackedGameFormat Format,
    TrackedGameStatus Status,
    TrackedGameOwnership Ownership
);

public static class GetAllUserTrackedGamesMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<TrackedGame, GetAllUserTrackedGamesItemResult>();
    }
}

public class GetAllUserTrackedGamesHandler : IRequestHandler<GetAllUserTrackedGamesQuery, PagedListResult<GetAllUserTrackedGamesItemResult>>
{
    private readonly DatabaseContext _databaseContext;
    private readonly IMapper _mapper;

    public GetAllUserTrackedGamesHandler(DatabaseContext databaseContext, IMapper mapper)
    {
        _databaseContext = databaseContext;
        _mapper = mapper;
    }
    
    public async Task<PagedListResult<GetAllUserTrackedGamesItemResult>> Handle(GetAllUserTrackedGamesQuery query, CancellationToken cancellationToken)
    {
        var queryable = _databaseContext.TrackedGames
            .AsNoTracking()
            .Where(tg => tg.UserRemoteId == query.UserRemoteId);

        if (query.GameStatus != null) queryable = queryable.Where(tg => tg.Status == query.GameStatus);
        if (query.SortByHoursPlayed) queryable = queryable.OrderBy(tg => tg.HoursPlayed);
        if (query.SortByPlatform) queryable = queryable.OrderBy(tg => tg.Platform);
        if (query.SortByFormat) queryable = queryable.OrderBy(tg => tg.Format);
        if (query.SortByOwnership) queryable = queryable.OrderBy(tg => tg.Ownership);

        var projectedQueryable = queryable.ProjectTo<GetAllUserTrackedGamesItemResult>(_mapper.ConfigurationProvider);
        
        var pagedList = await PagedListResult<GetAllUserTrackedGamesItemResult>.CreateAsync(
            projectedQueryable,
            query.Page,
            query.PageSize,
            cancellationToken
        );

        return pagedList;
    }
}