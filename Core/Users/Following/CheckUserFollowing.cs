using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Users.Following;

public record CheckUserFollowingQuery(
    string FollowerUserId,
    string FollowingUserId
) : IRequest<CheckUserFollowingResult>;

public class CheckUserFollowingValidator : AbstractValidator<CheckUserFollowingQuery>
{
    public CheckUserFollowingValidator()
    {
        RuleFor(q => q.FollowerUserId).NotEmpty();
        RuleFor(q => q.FollowingUserId).NotEmpty();
    }
}

public record CheckUserFollowingResult(bool IsFollowing);

public class CheckUserFollowingHandler : IRequestHandler<CheckUserFollowingQuery, CheckUserFollowingResult>
{
    private readonly DatabaseContext _databaseContext;

    public CheckUserFollowingHandler(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }
    
    public async Task<CheckUserFollowingResult> Handle(CheckUserFollowingQuery query,
        CancellationToken cancellationToken)
    {
        var isFollowing = await _databaseContext.Follows
            .Where(f => f.FollowerUserId == query.FollowerUserId
                        && f.FollowingUserId == query.FollowingUserId)
            .AnyAsync(cancellationToken);

        return new CheckUserFollowingResult(isFollowing);
    }
}