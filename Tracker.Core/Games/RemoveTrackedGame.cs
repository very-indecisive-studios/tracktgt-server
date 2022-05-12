using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tracker.Core.Exceptions;
using Tracker.Domain;
using Tracker.Persistence;

namespace Tracker.Core.Games;

public record RemoveTrackedGameCommand(
    string UserRemoteId,
    long GameRemoteId    
) : IRequest<Unit>;

public class RemoveTrackedGameValidator : AbstractValidator<RemoveTrackedGameCommand>
{
    public RemoveTrackedGameValidator()
    {
        RuleFor(c => c.UserRemoteId).NotEmpty();
    }
}

public class RemoveTrackedGameHandler : IRequestHandler<RemoveTrackedGameCommand, Unit>
{
    private readonly DatabaseContext _databaseContext;

    public RemoveTrackedGameHandler(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }
    
    public async Task<Unit> Handle(RemoveTrackedGameCommand command, CancellationToken cancellationToken)
    {
        TrackedGame? trackedGame = await _databaseContext.TrackedGames
            .Where(tg => tg.GameRemoteId == command.GameRemoteId && tg.UserRemoteId == command.UserRemoteId)
            .FirstOrDefaultAsync(cancellationToken);

        if (trackedGame == null)
        {
            throw new NotFoundException("Tracked game not found!");
        }

        _databaseContext.TrackedGames.Remove(trackedGame);
        await _databaseContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}