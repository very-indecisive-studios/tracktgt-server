using Core.Common;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Books.Tracking;

public class GetAllBookTrackingsQuery : PagedListRequest, IRequest<PagedListResult<GetAllBookTrackingsItemResult>>
{
    public string UserRemoteId { get; set; } = "";
    
    public BookTrackingStatus? BookStatus { get; set; } = null;

    public bool SortByRecentlyModified { get; set; } = false;
    
    public bool SortByChaptersRead { get; set; } = false;

    public bool SortByFormat { get; set; } = false;
    
    public bool SortByOwnership { get; set; } = false;
}

public class GetAllBookTrackingsValidator : AbstractValidator<GetAllBookTrackingsQuery>
{
    public GetAllBookTrackingsValidator()
    {
        RuleFor(q => q.UserRemoteId).NotEmpty();
    }    
}

public record GetAllBookTrackingsItemResult(
    string BookRemoteId,
    string Title,
    string CoverImageURL,
    int ChaptersRead,
    BookTrackingFormat Format,
    BookTrackingStatus Status,
    BookTrackingOwnership Ownership
);

public class GetAllBookTrackingsHandler : IRequestHandler<GetAllBookTrackingsQuery, PagedListResult<GetAllBookTrackingsItemResult>>
{
    private readonly DatabaseContext _databaseContext;

    public GetAllBookTrackingsHandler(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }
    
    public async Task<PagedListResult<GetAllBookTrackingsItemResult>> Handle(GetAllBookTrackingsQuery query, CancellationToken cancellationToken)
    {
        var queryable = _databaseContext.BookTrackings
            .AsNoTracking()
            .Where(bt => bt.UserRemoteId == query.UserRemoteId);

        if (query.BookStatus != null) queryable = queryable.Where(bt => bt.Status == query.BookStatus);
        if (query.SortByRecentlyModified) queryable = queryable.OrderByDescending(bt => bt.LastModifiedOn);
        if (query.SortByChaptersRead) queryable = queryable.OrderBy(bt => bt.ChaptersRead);
        if (query.SortByFormat) queryable = queryable.OrderBy(bt => bt.Format);
        if (query.SortByOwnership) queryable = queryable.OrderBy(bt => bt.Ownership);

        var joinQueryable = queryable.Join(
            _databaseContext.Books,
            bt => bt.BookRemoteId,
            b => b.RemoteId,
            (bt, b) => new GetAllBookTrackingsItemResult(
                bt.BookRemoteId,
                b.Title,
                b.CoverImageURL,
                bt.ChaptersRead,
                bt.Format,
                bt.Status,
                bt.Ownership
            )
        );
        
        var pagedList = await PagedListResult<GetAllBookTrackingsItemResult>.CreateAsync(
            joinQueryable,
            query.Page,
            query.PageSize,
            cancellationToken
        );

        return pagedList;
    }
}