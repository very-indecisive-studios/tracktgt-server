using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain;
using Domain.Tracking;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Games.Tracking;

public record GetGameTrackingsQuery(
    string UserRemoteId,
    long GameRemoteId
) : IRequest<GetGameTrackingsResult>;

public class GetGameTrackingsValidator : AbstractValidator<GetGameTrackingsQuery>
{
    public GetGameTrackingsValidator()
    {
        RuleFor(q => q.UserRemoteId).NotEmpty();
    }    
}

public record GetGameTrackingsResult(List<GetGameTrackingsResult.GetGameTrackingsItemResult> Items)
{
    public record GetGameTrackingsItemResult(
        float HoursPlayed,
        string Platform,
        GameTrackingFormat Format,
        GameTrackingStatus Status,
        GameTrackingOwnership Ownership
    );
}

public static class GetGameTrackingsMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<GameTracking, GetGameTrackingsResult.GetGameTrackingsItemResult>();
    }
}

public class GetGameTrackingsHandler : IRequestHandler<GetGameTrackingsQuery, GetGameTrackingsResult>
{
    private readonly DatabaseContext _databaseContext;
    private readonly IMapper _mapper;

    public GetGameTrackingsHandler(DatabaseContext databaseContext, IMapper mapper)
    {
        _databaseContext = databaseContext;
        _mapper = mapper;
    }
    
    public async Task<GetGameTrackingsResult> Handle(GetGameTrackingsQuery query, CancellationToken cancellationToken)
    {
        var gameTrackings = await _databaseContext.GameTrackings
            .AsNoTracking()
            .Where(gt => gt.UserRemoteId == query.UserRemoteId && gt.GameRemoteId == query.GameRemoteId)
            .ProjectTo<GetGameTrackingsResult.GetGameTrackingsItemResult>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
        
        return new GetGameTrackingsResult(gameTrackings);
    }
}