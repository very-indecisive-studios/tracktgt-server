using AutoMapper;
using Core.Exceptions;
using Domain;
using Domain.Media;
using Domain.Tracking;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Shows;

public record RemoveShowTrackingCommand(
    string UserRemoteId,
    string ShowRemoteId
) : IRequest<Unit>;

public class RemoveShowTrackingValidator : AbstractValidator<RemoveShowTrackingCommand>
{
    public RemoveShowTrackingValidator()
    {
        RuleFor(c => c.UserRemoteId).NotEmpty();
    }
}

public class RemoveShowTrackingHandler : IRequestHandler<RemoveShowTrackingCommand, Unit>
{
    private readonly DatabaseContext _databaseContext;
    
    public RemoveShowTrackingHandler(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }
    
    public async Task<Unit> Handle(RemoveShowTrackingCommand command, CancellationToken cancellationToken)
    {
        ShowTracking? showTracking = await _databaseContext.ShowTrackings
            .Where(showTracking => showTracking.ShowRemoteId == command.ShowRemoteId 
                                   && showTracking.UserRemoteId == command.UserRemoteId)
            .FirstOrDefaultAsync(cancellationToken);

        if (showTracking == null)
        {
            throw new NotFoundException("Tracked show not found!");
        }

        _databaseContext.ShowTrackings.Remove(showTracking);
        
        var show = await _databaseContext.Shows
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
        activity.Action = ActivityAction.Remove;
        _databaseContext.Activities.Add(activity);
        
        await _databaseContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}