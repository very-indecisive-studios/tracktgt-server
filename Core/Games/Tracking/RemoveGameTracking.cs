using Core.Exceptions;
using Domain;
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
        await _databaseContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}