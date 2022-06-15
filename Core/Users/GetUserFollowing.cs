using AutoMapper;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Users;

public record GetUserFollowingQuery(string CurrentUserId) : IRequest<GetUserFollowingResult>;

public class GetUserFollowingValidator : AbstractValidator<GetUserFollowingQuery>
{
    public GetUserFollowingValidator()
    {
        RuleFor(q => q.CurrentUserId).NotEmpty();
    }
}

public record GetUserFollowingResult(
    List<String> FollowingList
);

public class GetUserFollowingHandler : IRequestHandler<GetUserFollowingQuery, GetUserFollowingResult>
{
    private readonly DatabaseContext _databaseContext;

    public GetUserFollowingHandler(DatabaseContext databaseContext, IMapper mapper)
    {
        _databaseContext = databaseContext;
    }
    
    public async Task<GetUserFollowingResult> Handle(GetUserFollowingQuery query, CancellationToken cancellationToken)
    {
        var follows = await _databaseContext.Follows
            .AsNoTracking()
            .Where(f => f.FollowerUserId == query.CurrentUserId).ToListAsync(cancellationToken);

        List<String> followingList = new List<String>();
        foreach (var follow in follows)
        {
            followingList.Add(follow.FollowingUserId);
        }

        return new GetUserFollowingResult(followingList);
    }
}