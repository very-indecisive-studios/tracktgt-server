using AutoMapper;
using Domain.User;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Users.Following;

public record GetUserFollowingsQuery(string CurrentUserId) : IRequest<GetUserFollowingsResult>;

public class GetUserFollowingsValidator : AbstractValidator<GetUserFollowingsQuery>
{
    public GetUserFollowingsValidator()
    {
        RuleFor(q => q.CurrentUserId).NotEmpty();
    }
}

public record GetUserFollowingsResult(List<GetUserFollowingsResult.GetUserFollowingsItemResult> Items)
{
    public record GetUserFollowingsItemResult(
        string UserName, 
        string ProfilePictureURL
    );
}

public static class GetUserFollowingsMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<User, GetUserFollowingsResult.GetUserFollowingsItemResult>();
    }
}

public class GetUserFollowingsHandler : IRequestHandler<GetUserFollowingsQuery, GetUserFollowingsResult>
{
    private readonly DatabaseContext _databaseContext;
    private readonly IMapper _mapper;

    public GetUserFollowingsHandler(DatabaseContext databaseContext, IMapper mapper)
    {
        _databaseContext = databaseContext;
        _mapper = mapper;
    }
    
    public async Task<GetUserFollowingsResult> Handle(GetUserFollowingsQuery query, CancellationToken cancellationToken)
    {
        var followings = await _databaseContext.Follows
            .AsNoTracking()
            .Where(f => f.FollowerUserId == query.CurrentUserId)
            .Join(
                _databaseContext.Users,
                f => f.FollowingUserId,
                u => u.RemoteId,
                (f, u) => _mapper.Map<User, GetUserFollowingsResult.GetUserFollowingsItemResult>(u)
            )
            .ToListAsync(cancellationToken);

        return new GetUserFollowingsResult(followings);
    }
}