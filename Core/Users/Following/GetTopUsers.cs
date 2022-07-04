using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Users.Following;

public record GetTopUsersQuery(int NoOfUsers) : IRequest<GetTopUsersResult>;

public class GetTopUsersValidator : AbstractValidator<GetTopUsersQuery>
{
    public GetTopUsersValidator()
    {
        RuleFor(q => q.NoOfUsers).NotEmpty();
    }
}

public record GetTopUsersResult(List<GetTopUsersResult.GetTopUsersItemResult> Items)
{
    public record GetTopUsersItemResult(
        string RemoteId,
        string UserName,
        string ProfilePictureURL,
        string Bio,
        int FollowersCount
    );
}

public class GetTopUsersHandler : IRequestHandler<GetTopUsersQuery, GetTopUsersResult>
{
    private readonly DatabaseContext _databaseContext;

    public GetTopUsersHandler(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }
    
    public async Task<GetTopUsersResult> Handle(GetTopUsersQuery query, CancellationToken cancellationToken)
    {
        var listOfFollows = await _databaseContext.Follows
            .GroupBy(q => q.FollowingUserId)
            .OrderByDescending(g => g.Count())
            .Take(query.NoOfUsers)
            .Select(g => new { UserRemoteId = g.Key, FollowersCount = g.Count()})
            .Join(
                _databaseContext.Users,
                g => g.UserRemoteId,
                u => u.RemoteId,
                (g, u) => new GetTopUsersResult.GetTopUsersItemResult(
                    u.RemoteId,
                    u.UserName,
                    u.ProfilePictureURL,
                    u.Bio,
                    g.FollowersCount
                )
            )
            .ToListAsync(cancellationToken);
        
        
        return new GetTopUsersResult(listOfFollows);
    }
}
