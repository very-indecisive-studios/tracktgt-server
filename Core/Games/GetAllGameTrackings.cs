using AutoMapper;
using Core.Common;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Domain;
using Persistence;

namespace Core.Games;

public class GetAllGameTrackingsQuery : PagedListRequest, IRequest<PagedListResult<GetAllGameTrackingsItemResult>>
{
    public string UserRemoteId { get; set; } = "";
    
    public GameTrackingStatus? GameStatus { get; set; } = null;

    public bool SortByRecentlyModified { get; set; } = false;
    
    public bool SortByHoursPlayed { get; set; } = false;
    
    public bool SortByPlatform { get; set; } = false;
    
    public bool SortByFormat { get; set; } = false;
    
    public bool SortByOwnership { get; set; } = false;
}

public class GetAllGameTrackingsValidator : AbstractValidator<GetAllGameTrackingsQuery>
{
    public GetAllGameTrackingsValidator()
    {
        RuleFor(q => q.UserRemoteId).NotEmpty();
    }    
}

public record GetAllGameTrackingsItemResult(
    long GameRemoteId,
    string Title,
    string CoverImageURL,
    float HoursPlayed,
    string Platform,
    GameTrackingFormat Format,
    GameTrackingStatus Status,
    GameTrackingOwnership Ownership
);

public class GetAllGameTrackingsHandler : IRequestHandler<GetAllGameTrackingsQuery, PagedListResult<GetAllGameTrackingsItemResult>>
{
    private readonly DatabaseContext _databaseContext;
    private readonly IMapper _mapper;

    public GetAllGameTrackingsHandler(DatabaseContext databaseContext, IMapper mapper)
    {
        _databaseContext = databaseContext;
        _mapper = mapper;
    }
    
    public async Task<PagedListResult<GetAllGameTrackingsItemResult>> Handle(GetAllGameTrackingsQuery query, CancellationToken cancellationToken)
    {
        var queryable = _databaseContext.GameTrackings
            .AsNoTracking()
            .Where(tg => tg.UserRemoteId == query.UserRemoteId);

        if (query.GameStatus != null) queryable = queryable.Where(gt => gt.Status == query.GameStatus);
        if (query.SortByRecentlyModified) queryable = queryable.OrderByDescending(gt => gt.LastModifiedOn);
        if (query.SortByHoursPlayed) queryable = queryable.OrderBy(gt => gt.HoursPlayed);
        if (query.SortByPlatform) queryable = queryable.OrderBy(gt => gt.Platform);
        if (query.SortByFormat) queryable = queryable.OrderBy(gt => gt.Format);
        if (query.SortByOwnership) queryable = queryable.OrderBy(gt => gt.Ownership);

        var joinQueryable = queryable.Join(
            _databaseContext.Games,
            gt => gt.GameRemoteId,
            g => g.RemoteId,
            (gt, g) => new GetAllGameTrackingsItemResult(
                gt.GameRemoteId,
                g.Title,
                g.CoverImageURL,
                gt.HoursPlayed,
                gt.Platform,
                gt.Format,
                gt.Status,
                gt.Ownership
            )
        );
        
        var pagedList = await PagedListResult<GetAllGameTrackingsItemResult>.CreateAsync(
            joinQueryable,
            query.Page,
            query.PageSize,
            cancellationToken
        );

        return pagedList;
    }
}