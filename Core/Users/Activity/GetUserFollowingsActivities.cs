using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Users.Activity;

public record GetUserFollowingsActivitiesQuery(string UserRemoteId) : IRequest<GetUserFollowingsActivitiesResult>;

public class GetUserFollowingsActivitiesValidator : AbstractValidator<GetUserFollowingsActivitiesQuery>
{
    public GetUserFollowingsActivitiesValidator()
    {
        RuleFor(q => q.UserRemoteId).NotEmpty();
    }
}

public record GetUserFollowingsActivitiesResult(
    List<GetUserFollowingsActivitiesResult.GetUserFollowingsActivitiesItemResult> Items
)
{
    public record GetUserFollowingsActivitiesItemResult(
        string Id,
        string UserName,
        string ProfilePictureURL,
        string MediaRemoteId,
        string MediaTitle,
        string MediaCoverImageURL,
        string Status,
        int NoOf,
        ActivityMediaType MediaType,
        ActivityAction Action,
        DateTime DateTime
    );
}

public class GetUserFollowingsActivitiesHandler : IRequestHandler<GetUserFollowingsActivitiesQuery, GetUserFollowingsActivitiesResult>
{
    private readonly DatabaseContext _databaseContext;

    public GetUserFollowingsActivitiesHandler(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }
    
    public async Task<GetUserFollowingsActivitiesResult> Handle(
        GetUserFollowingsActivitiesQuery query,
        CancellationToken cancellationToken
    )
    {
        var activities = await _databaseContext.Activities
            .AsNoTracking()
            .Where(a => _databaseContext.Follows
                .Any(f => f.FollowerUserId.Equals(query.UserRemoteId) && f.FollowingUserId.Equals(a.UserRemoteId)))
            .OrderByDescending(a => a.CreatedOn)
            .Take(20)
            .Join(
                _databaseContext.Users,
                a => a.UserRemoteId,
                u => u.RemoteId,
                (a, u) => new GetUserFollowingsActivitiesResult.GetUserFollowingsActivitiesItemResult(
                    a.Id.ToString(),
                    u.UserName,
                    u.ProfilePictureURL,
                    a.MediaRemoteId,
                    a.MediaTitle,
                    a.MediaCoverImageURL,
                    a.Status,
                    a.NoOf,
                    a.MediaType,
                    a.Action,
                    a.CreatedOn
                )
            )
            .ToListAsync(cancellationToken);

        return new GetUserFollowingsActivitiesResult(activities);
    }
}