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
        string MediaRemoteId,
        string MediaTitle,
        string MediaCoverImageURL,
        string Status,
        int NoOf,
        ActivityMediaType MediaType,
        ActivityAction Action
    );
}

public static class GetUserActivitiesMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<Domain.Activity, GetUserActivitiesResult.GetUserActivitiesItemResult>();
    }
}

public class GetUserActivitiesHandler : IRequestHandler<GetUserActivitiesQuery, GetUserActivitiesResult>
{
    private readonly DatabaseContext _databaseContext;
    private readonly IMapper _mapper;

    public GetUserActivitiesHandler(DatabaseContext databaseContext, IMapper mapper)
    {
        _databaseContext = databaseContext;
        _mapper = mapper;
    }
    
    public async Task<GetUserActivitiesResult> Handle(GetUserActivitiesQuery query, CancellationToken cancellationToken)
    {
        var activities = await _databaseContext.Activities
            .AsNoTracking()
            .Where(a => a.UserRemoteId == query.UserRemoteId)
            .OrderByDescending(a => a.CreatedOn)
            .Take(10)
            .ProjectTo<GetUserActivitiesResult.GetUserActivitiesItemResult>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
        
        return new (activities);
    }
}