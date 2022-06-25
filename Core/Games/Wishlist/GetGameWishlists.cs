using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain;
using Domain.Wishlist;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Games.Wishlist;

public record GetGameWishlistsQuery(
    string UserRemoteId,
    long GameRemoteId
) : IRequest<GetGameWishlistsResult>;

public class GetGameWishlistsValidator : AbstractValidator<GetGameWishlistsQuery>
{
    public GetGameWishlistsValidator()
    {
        RuleFor(q => q.UserRemoteId).NotEmpty();
    }
}

public record GetGameWishlistsResult(List<GetGameWishlistsResult.GetGameWishlistItemResult> Items)
{
    public record GetGameWishlistItemResult(
        string Platform
    );
};

public static class GetGameWishlistsMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<GameWishlist, GetGameWishlistsResult.GetGameWishlistItemResult>();
    }
}

public class GetGameWishlistsHandler : IRequestHandler<GetGameWishlistsQuery, GetGameWishlistsResult>
{
    private readonly DatabaseContext _databaseContext;
    private readonly IMapper _mapper;

    public GetGameWishlistsHandler(DatabaseContext databaseContext, IMapper mapper)
    {
        _databaseContext = databaseContext;
        _mapper = mapper;
    }
    
    public async Task<GetGameWishlistsResult> Handle(GetGameWishlistsQuery query, CancellationToken cancellationToken)
    {
        var gameWishlists = await _databaseContext.GameWishlists
            .AsNoTracking()
            .Where(gw => gw.UserRemoteId == query.UserRemoteId && gw.GameRemoteId == query.GameRemoteId)
            .ProjectTo<GetGameWishlistsResult.GetGameWishlistItemResult>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
        
        return new GetGameWishlistsResult(gameWishlists);
    }
}