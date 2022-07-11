using AutoMapper;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Users.Activity;

public record GetGlobalActivitiesQuery() : IRequest<GetGlobalActivitiesResult>;

public record GetGlobalActivitiesResult(List<GetGlobalActivitiesResult.GetGlobalActivitiesItemResult> Items)
{
    public record GetGlobalActivitiesItemResult(
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

public class GetGlobalActivitiesHandler : IRequestHandler<GetGlobalActivitiesQuery, GetGlobalActivitiesResult>
{
    private readonly DatabaseContext _databaseContext;

    public GetGlobalActivitiesHandler(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }
    
    public async Task<GetGlobalActivitiesResult> Handle(GetGlobalActivitiesQuery query,
        CancellationToken cancellationToken)
    {
        var activities = await _databaseContext.Activities
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedOn)
            .Take(20)
            .Join(
                _databaseContext.Users,
                a => a.UserRemoteId,
                u => u.RemoteId,
                (a, u) => new GetGlobalActivitiesResult.GetGlobalActivitiesItemResult(
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
        
        return new (activities);
    }
}