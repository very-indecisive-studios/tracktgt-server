using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Shows;

public record GetShowTrackingsQuery(
    string UserRemoteId,
    int ShowRemoteId
) : IRequest<GetShowTrackingsResult>;

public class GetShowTrackingsValidator : AbstractValidator<GetShowTrackingsQuery>
{
    public GetShowTrackingsValidator()
    {
        RuleFor(q => q.UserRemoteId).NotEmpty();
    }
}

public record GetShowTrackingsResult(List<GetShowTrackingsResult.GetShowTrackingsItemResult> Items)
{
    public record GetShowTrackingsItemResult(
        int EpisodesWatched,
        ShowType ShowType,
        ShowTrackingStatus Status
    );
}

public static class GetShowTrackingsMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<ShowTracking, GetShowTrackingsResult.GetShowTrackingsItemResult>();
    }
}

public class GetShowTrackingsHandler : IRequestHandler<GetShowTrackingsQuery, GetShowTrackingsResult>
{
    private readonly DatabaseContext _databaseContext;
    private readonly IMapper _mapper;

    public GetShowTrackingsHandler(DatabaseContext databaseContext, IMapper mapper)
    {
        _databaseContext = databaseContext;
        _mapper = mapper;
    }
    
    public async Task<GetShowTrackingsResult> Handle(GetShowTrackingsQuery query, CancellationToken cancellationToken)
    {
        var showTrackings = await _databaseContext.ShowTrackings
            .AsNoTracking()
            .Where(showTracking => showTracking.UserRemoteId == query.UserRemoteId && showTracking.ShowRemoteId == query.ShowRemoteId)
            .ProjectTo<GetShowTrackingsResult.GetShowTrackingsItemResult>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
        
        return new GetShowTrackingsResult(showTrackings);

    }
}