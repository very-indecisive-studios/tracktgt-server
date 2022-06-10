using Core.Common;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Books.Wishlist;

public class GetAllBookWishlistsQuery : PagedListRequest, IRequest<PagedListResult<GetAllBookWishlistsItemResult>>
{
    public string UserRemoteId { get; set; } = "";
}

public class GetAllBookWishlistsValidator : AbstractValidator<GetAllBookWishlistsQuery>
{
    public GetAllBookWishlistsValidator()
    {
        RuleFor(q => q.UserRemoteId).NotEmpty();
    }    
}

public record GetAllBookWishlistsItemResult(
    string BookRemoteId,
    string Title,
    string CoverImageURL
);

public class GetAllBookWishlistsHandler : IRequestHandler<GetAllBookWishlistsQuery, PagedListResult<GetAllBookWishlistsItemResult>>
{
    private readonly DatabaseContext _databaseContext;

    public GetAllBookWishlistsHandler(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }
    
    public async Task<PagedListResult<GetAllBookWishlistsItemResult>> Handle(GetAllBookWishlistsQuery query, CancellationToken cancellationToken)
    {
        var queryable = _databaseContext.BookWishlists
            .AsNoTracking()
            .Where(bt => bt.UserRemoteId == query.UserRemoteId);

        var joinQueryable = queryable.Join(
            _databaseContext.Books,
            bw => bw.BookRemoteId,
            b => b.RemoteId,
            (bt, b) => new GetAllBookWishlistsItemResult(
                bt.BookRemoteId,
                b.Title,
                b.CoverImageURL
            )
        );
        
        var pagedList = await PagedListResult<GetAllBookWishlistsItemResult>.CreateAsync(
            joinQueryable,
            query.Page,
            query.PageSize,
            cancellationToken
        );

        return pagedList;
    }
}