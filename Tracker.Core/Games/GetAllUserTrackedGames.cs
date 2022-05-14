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
    public string UserRemoteId { get; set; } = "";
    
    public TrackedGameStatus? GameStatus { get; set; } = null;
    
    public bool SortByHoursPlayed { get; set; } = false;
    
    public bool SortByPlatform { get; set; } = false;
    
    public bool SortByFormat { get; set; } = false;
    
    public bool SortByOwnership { get; set; } = false;
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
    string Title,
    float HoursPlayed,
    string Platform,
    TrackedGameFormat Format,
    TrackedGameStatus Status,
    TrackedGameOwnership Ownership
);

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

        var joinQueryable = queryable.Join(
            _databaseContext.Games,
            tg => tg.GameRemoteId,
            g => g.RemoteId,
            (tg, g) => new GetAllUserTrackedGamesItemResult(
                tg.GameRemoteId,
                g.Title,
                tg.HoursPlayed,
                tg.Platform,
                tg.Format,
                tg.Status,
                tg.Ownership
            )
        );
        
        var pagedList = await PagedListResult<GetAllUserTrackedGamesItemResult>.CreateAsync(
            joinQueryable,
            query.Page,
            query.PageSize,
            cancellationToken
        );

        return pagedList;
    }
}