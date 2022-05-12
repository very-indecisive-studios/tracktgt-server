using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tracker.Core.Common;
using Tracker.Domain;
using Tracker.Persistence;

namespace Tracker.Core.Games;

public class GetTrackedGamesQuery : PagedListRequest, IRequest<PagedListResult<GetTrackedGamesItemResult>>
{
    public GetTrackedGamesQuery(string userRemoteId)
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

public class GetTrackedGamesValidator : AbstractValidator<GetTrackedGamesQuery>
{
    public GetTrackedGamesValidator()
    {
        RuleFor(q => q.UserRemoteId).NotEmpty();
    }    
}

public record GetTrackedGamesItemResult(
    long GameRemoteId,
    float HoursPlayed,
    string Platform,
    TrackedGameFormat Format,
    TrackedGameStatus Status,
    TrackedGameOwnership Ownership
);

public static class GetTrackedGamesMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<TrackedGame, GetTrackedGamesItemResult>();
    }
}

public class GetTrackedGamesHandler : IRequestHandler<GetTrackedGamesQuery, PagedListResult<GetTrackedGamesItemResult>>
{
    private readonly DatabaseContext _databaseContext;
    private readonly IMapper _mapper;

    public GetTrackedGamesHandler(DatabaseContext databaseContext, IMapper mapper)
    {
        _databaseContext = databaseContext;
        _mapper = mapper;
    }
    
    public async Task<PagedListResult<GetTrackedGamesItemResult>> Handle(GetTrackedGamesQuery query, CancellationToken cancellationToken)
    {
        var queryable = _databaseContext.TrackedGames
            .AsNoTracking()
            .Where(tg => tg.UserRemoteId == query.UserRemoteId);

        if (query.GameStatus != null) queryable = queryable.Where(tg => tg.Status == query.GameStatus);
        if (query.SortByHoursPlayed) queryable = queryable.OrderBy(tg => tg.HoursPlayed);
        if (query.SortByPlatform) queryable = queryable.OrderBy(tg => tg.Platform);
        if (query.SortByFormat) queryable = queryable.OrderBy(tg => tg.Format);
        if (query.SortByOwnership) queryable = queryable.OrderBy(tg => tg.Ownership);

        var projectedQueryable = queryable.ProjectTo<GetTrackedGamesItemResult>(_mapper.ConfigurationProvider);
        
        var pagedList = await PagedListResult<GetTrackedGamesItemResult>.CreateAsync(
            projectedQueryable,
            query.Page,
            query.PageSize,
            cancellationToken
        );

        return pagedList;
    }
}