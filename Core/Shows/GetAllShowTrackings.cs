using AutoMapper;
using Core.Common;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Shows;

public class GetAllShowTrackingsQuery : PagedListRequest, IRequest<PagedListResult<GetAllShowTrackingsItemResult>>
{
    public string UserRemoteId { get; set; } = "";
    
    public ShowTrackingStatus? Status { get; set; } = null;

    public bool SortByRecentlyModified { get; set; } = false;
    
    public bool SortByEpisodesWatched { get; set; } = false;
}

public class GetAllShowTrackingsValidator : AbstractValidator<GetAllShowTrackingsQuery>
{
    public GetAllShowTrackingsValidator()
    {
        RuleFor(q => q.UserRemoteId).NotEmpty();
    }
}

public record GetAllShowTrackingsItemResult(
    int ShowRemoteId,
    string Title,
    string CoverImageURL,
    int EpisodesWatched,
    ShowType ShowType,
    ShowTrackingStatus Status
);

public class GetAllShowTrackingsHandler : IRequestHandler<GetAllShowTrackingsQuery, PagedListResult<GetAllShowTrackingsItemResult>>
{
    private readonly DatabaseContext _databaseContext;
    private readonly IMapper _mapper;
    
    public GetAllShowTrackingsHandler(DatabaseContext databaseContext, IMapper mapper)
    {
        _databaseContext = databaseContext;
        _mapper = mapper;
    }
    
    public async Task<PagedListResult<GetAllShowTrackingsItemResult>> Handle(GetAllShowTrackingsQuery query, CancellationToken cancellationToken)
    {
        var queryable = _databaseContext.ShowTrackings
            .AsNoTracking()
            .Where(showTracking => showTracking.UserRemoteId == query.UserRemoteId);
        
        if (query.Status != null) queryable = queryable.Where(showTracking => showTracking.Status == query.Status);
        if (query.SortByRecentlyModified) queryable = queryable.OrderByDescending(showTracking => showTracking.LastModifiedOn);
        if (query.SortByEpisodesWatched) queryable = queryable.OrderBy(showTracking => showTracking.EpisodesWatched);
        
        var joinQueryable = queryable.Join(
            _databaseContext.Shows,
            showTracking => showTracking.ShowRemoteId,
            show => show.RemoteId,
            (showTracking, show) => new GetAllShowTrackingsItemResult(
                showTracking.ShowRemoteId,
                show.Title,
                show.CoverImageURL,
                showTracking.EpisodesWatched,
                show.ShowType,
                showTracking.Status
            )
        );
        
        var pagedList = await PagedListResult<GetAllShowTrackingsItemResult>.CreateAsync(
            joinQueryable,
            query.Page,
            query.PageSize,
            cancellationToken
        );

        return pagedList;
    }
}