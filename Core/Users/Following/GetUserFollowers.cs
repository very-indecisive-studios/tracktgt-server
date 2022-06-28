using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Users.Following;

public record GetUserFollowersQuery(string CurrentUserId) : IRequest<GetUserFollowersResult>;

public class GetUserFollowersValidator : AbstractValidator<GetUserFollowersQuery>
{
    public GetUserFollowersValidator()
    {
        RuleFor(q => q.CurrentUserId).NotEmpty();
    }
}

public record GetUserFollowersResult(
    List<String> FollowersList
);

public class GetUserFollowersHandler : IRequestHandler<GetUserFollowersQuery, GetUserFollowersResult>
{
    private readonly DatabaseContext _databaseContext;

    public GetUserFollowersHandler(DatabaseContext databaseContext, IMapper mapper)
    {
        _databaseContext = databaseContext;
    }
    
    public async Task<GetUserFollowersResult> Handle(GetUserFollowersQuery query, CancellationToken cancellationToken)
    {
        var follows = await _databaseContext.Follows
            .AsNoTracking()
            .Where(f => f.FollowerUserId == query.CurrentUserId).ToListAsync(cancellationToken);

        List<String> followersList = new List<String>();
        foreach (var follow in follows)
        {
            followersList.Add(follow.FollowingUserId);
        }

        return new GetUserFollowersResult(followersList);
    }
}