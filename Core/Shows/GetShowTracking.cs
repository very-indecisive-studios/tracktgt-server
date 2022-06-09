using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Shows;

public record GetShowTrackingQuery(
    string UserRemoteId,
    string ShowRemoteId
) : IRequest<GetShowTrackingResult?>;

public class GetShowTrackingValidator : AbstractValidator<GetShowTrackingQuery>
{
    public GetShowTrackingValidator()
    {
        RuleFor(q => q.UserRemoteId).NotEmpty();
        RuleFor(q => q.ShowRemoteId).NotEmpty();
    }
}

public record GetShowTrackingResult(
    int EpisodesWatched,
    ShowType ShowType,
    ShowTrackingStatus Status
);

public static class GetShowTrackingMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<ShowTracking, GetShowTrackingResult>();
    }
}

public class GetShowTrackingHandler : IRequestHandler<GetShowTrackingQuery, GetShowTrackingResult?>
{
    private readonly DatabaseContext _databaseContext;
    private readonly IMapper _mapper;

    public GetShowTrackingHandler(DatabaseContext databaseContext, IMapper mapper)
    {
        _databaseContext = databaseContext;
        _mapper = mapper;
    }
    
    public async Task<GetShowTrackingResult?> Handle(GetShowTrackingQuery query, CancellationToken cancellationToken)
    {
        var showTracking = await _databaseContext.ShowTrackings
            .AsNoTracking()
            .Where(showTracking => showTracking.UserRemoteId == query.UserRemoteId 
                                   && showTracking.ShowRemoteId == query.ShowRemoteId)
            .ProjectTo<GetShowTrackingResult>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
        
        return showTracking;
    }
}