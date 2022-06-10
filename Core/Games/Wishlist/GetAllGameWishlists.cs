using Core.Common;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Games.Wishlist;

public class GetAllGameWishlistsQuery : PagedListRequest, IRequest<PagedListResult<GetAllGameWishlistsItemResult>>
{
    public string UserRemoteId { get; set; } = "";
    
    public bool SortByPlatform { get; set; } = false;
}

public class GetAllGameWishlistsValidator : AbstractValidator<GetAllGameWishlistsQuery>
{
    public GetAllGameWishlistsValidator()
    {
        RuleFor(q => q.UserRemoteId).NotEmpty();
    }
}

public record GetAllGameWishlistsItemResult(
    long GameRemoteId,
    string Title,
    string CoverImageURL,
    string Platform
);

public class GetAllGameWishlistsHandler : IRequestHandler<GetAllGameWishlistsQuery, PagedListResult<GetAllGameWishlistsItemResult>>
{
    private readonly DatabaseContext _databaseContext;

    public GetAllGameWishlistsHandler(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }
    
    public async Task<PagedListResult<GetAllGameWishlistsItemResult>> Handle(GetAllGameWishlistsQuery query,
        CancellationToken cancellationToken)
    {
        var queryable = _databaseContext.GameWishlists
            .AsNoTracking()
            .Where(tg => tg.UserRemoteId == query.UserRemoteId);

        if (query.SortByPlatform) queryable = queryable.OrderBy(gt => gt.Platform);

        var joinQueryable = queryable.Join(
            _databaseContext.Games,
            gt => gt.GameRemoteId,
            g => g.RemoteId,
            (gt, g) => new GetAllGameWishlistsItemResult(
                gt.GameRemoteId,
                g.Title,
                g.CoverImageURL,
                gt.Platform
            )
        );
        
        var pagedList = await PagedListResult<GetAllGameWishlistsItemResult>.CreateAsync(
            joinQueryable,
            query.Page,
            query.PageSize,
            cancellationToken
        );

        return pagedList;
    }
}