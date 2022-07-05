using AutoMapper;
using AutoMapper.QueryableExtensions;
using Core.Exceptions;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Users.Activity;

public record GetUserActivitiesQuery(string UserRemoteId) : IRequest<GetUserActivitiesResult>;

public class GetUserActivitiesValidator : AbstractValidator<GetUserActivitiesQuery>
{
    public GetUserActivitiesValidator()
    {
        RuleFor(q => q.UserRemoteId).NotEmpty();
    }
}

public record GetUserActivitiesResult(List<GetUserActivitiesResult.GetUserActivitiesItemResult> Items)
{
    public record GetUserActivitiesItemResult(
        string UserName,
        string ProfilePictureURL,
        string MediaRemoteId,
        string MediaTitle,
        string MediaCoverImageURL,
        string Status,
        int NoOf,
        ActivityMediaType MediaType,
        ActivityAction Action
    );
}

public class GetUserActivitiesHandler : IRequestHandler<GetUserActivitiesQuery, GetUserActivitiesResult>
{
    private readonly DatabaseContext _databaseContext;

    public GetUserActivitiesHandler(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }
    
    public async Task<GetUserActivitiesResult> Handle(GetUserActivitiesQuery query, CancellationToken cancellationToken)
    {
        var activities = await _databaseContext.Activities
            .AsNoTracking()
            .Where(a => a.UserRemoteId == query.UserRemoteId)
            .OrderByDescending(a => a.CreatedOn)
            .Take(10)
            .Join(
                _databaseContext.Users,
                a => a.UserRemoteId,
                u => u.RemoteId,
                (a, u) => new GetUserActivitiesResult.GetUserActivitiesItemResult(
                    u.UserName,
                    u.ProfilePictureURL,
                    a.MediaRemoteId,
                    a.MediaTitle,
                    a.MediaCoverImageURL,
                    a.Status,
                    a.NoOf,
                    a.MediaType,
                    a.Action
                )
            )
            .ToListAsync(cancellationToken);
        
        return new (activities);
    }
}