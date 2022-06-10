using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Books.Wishlist;

public record GetBookWishlistQuery(
    string UserRemoteId,
    string BookRemoteId
) : IRequest<bool>;

public class GetBookWishlistValidator : AbstractValidator<GetBookWishlistQuery>
{
    public GetBookWishlistValidator()
    {
        RuleFor(q => q.UserRemoteId).NotEmpty();
        RuleFor(q => q.BookRemoteId).NotEmpty();
    }    
}

public static class GetBookWishlistMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<BookWishlist, bool>();
    }
}

public class GetBookWishlistHandler : IRequestHandler<GetBookWishlistQuery, bool>
{
    private readonly DatabaseContext _databaseContext;
    private readonly IMapper _mapper;

    public GetBookWishlistHandler(DatabaseContext databaseContext, IMapper mapper)
    {
        _databaseContext = databaseContext;
        _mapper = mapper;
    }
    
    public async Task<bool> Handle(GetBookWishlistQuery query, CancellationToken cancellationToken)
    {
        var hasBookWishlist = await _databaseContext.BookWishlists
            .AsNoTracking()
            .Where(bt => bt.UserRemoteId == query.UserRemoteId && bt.BookRemoteId == query.BookRemoteId)
            .AnyAsync(cancellationToken);

        return hasBookWishlist;
    }
}
