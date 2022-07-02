using Core.Exceptions;
using Domain;
using Domain.Tracking;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Games.Tracking;

public record RemoveGameTrackingCommand(
    string UserRemoteId,
    long GameRemoteId,
    string Platform
) : IRequest<Unit>;

public class RemoveGameTrackingValidator : AbstractValidator<RemoveGameTrackingCommand>
{
    public RemoveGameTrackingValidator()
    {
        RuleFor(c => c.UserRemoteId).NotEmpty();
        RuleFor(c => c.Platform).NotEmpty();
    }
}

public class RemoveGameTrackingHandler : IRequestHandler<RemoveGameTrackingCommand, Unit>
{
    private readonly DatabaseContext _databaseContext;

    public RemoveGameTrackingHandler(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }
    
    public async Task<Unit> Handle(RemoveGameTrackingCommand command, CancellationToken cancellationToken)
    {
        GameTracking? gameTracking = await _databaseContext.GameTrackings
            .Where(tg => tg.GameRemoteId == command.GameRemoteId 
                         && tg.UserRemoteId == command.UserRemoteId
                         && tg.Platform.Equals(command.Platform))
            .FirstOrDefaultAsync(cancellationToken);

        if (gameTracking == null)
        {
            throw new NotFoundException("Tracked game not found!");
        }

        _databaseContext.GameTrackings.Remove(gameTracking);

        var game = await _databaseContext.Games
            .AsNoTracking()
            .Where(g => g.RemoteId == command.GameRemoteId)
            .FirstOrDefaultAsync(cancellationToken);
        if (game == null)
        {
            throw new NotFoundException("Game not found!");
        }
        
        Activity activity = new Activity();
        activity.UserRemoteId = command.UserRemoteId;
        activity.Status = gameTracking.Status.ToString();
        activity.NoOf = (int) gameTracking.HoursPlayed;
        activity.MediaRemoteId = game.RemoteId.ToString();
        activity.MediaTitle = game.Title;
        activity.MediaCoverImageURL = game.CoverImageURL;
        activity.MediaType = ActivityMediaType.Game;
        activity.Action = ActivityAction.Remove;
        _databaseContext.Activities.Add(activity);
        
        await _databaseContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}