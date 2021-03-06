using AutoMapper;
using Core.Exceptions;
using Domain;
using Domain.Tracking;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Shows;

public record UpdateShowTrackingCommand(
    string UserRemoteId,
    string ShowRemoteId, 
    int EpisodesWatched,
    ShowTrackingStatus Status
) : IRequest<Unit>;

public class UpdateShowTrackingValidator : AbstractValidator<UpdateShowTrackingCommand>
{
    public UpdateShowTrackingValidator()
    {
        RuleFor(c => c.UserRemoteId).NotEmpty();
        RuleFor(c => c.ShowRemoteId).NotEmpty();
    }
}

public static class UpdateShowTrackingMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<UpdateShowTrackingCommand, ShowTracking>();
    }
}

public class UpdateShowTrackingHandler : IRequestHandler<UpdateShowTrackingCommand, Unit>
{
    private readonly DatabaseContext _dbContext;
    private readonly IMapper _mapper;

    public UpdateShowTrackingHandler(DatabaseContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }
    
    public async Task<Unit> Handle(UpdateShowTrackingCommand command, CancellationToken cancellationToken)
    {
        ShowTracking? showTracking = await _dbContext.ShowTrackings
            .Where(showTracking => showTracking.ShowRemoteId == command.ShowRemoteId
                                   && showTracking.UserRemoteId == command.UserRemoteId)
            .FirstOrDefaultAsync(cancellationToken);

        if (showTracking == null)
        {
            throw new NotFoundException();
        }

        _mapper.Map<UpdateShowTrackingCommand, ShowTracking>(command, showTracking);
        _dbContext.ShowTrackings.Update(showTracking);
        
        var show = await _dbContext.Shows
            .AsNoTracking()
            .Where(show => show.RemoteId == command.ShowRemoteId)
            .FirstOrDefaultAsync(cancellationToken);
        if (show == null)
        {
            throw new NotFoundException("Show not found!");
        }
        
        Activity activity = new Activity();
        activity.UserRemoteId = showTracking.UserRemoteId;
        activity.Status = showTracking.Status.ToString();
        activity.NoOf = showTracking.EpisodesWatched;
        activity.MediaRemoteId = show.RemoteId;
        activity.MediaTitle = show.Title;
        activity.MediaCoverImageURL = show.CoverImageURL;
        activity.MediaType = ActivityMediaType.Show;
        activity.Action = ActivityAction.Update;
        _dbContext.Activities.Add(activity);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}