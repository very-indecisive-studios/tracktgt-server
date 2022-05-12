using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tracker.Core.Exceptions;
using Tracker.Domain;
using Tracker.Persistence;

namespace Tracker.Core.Games;

public record DeleteTrackedGameCommand(
    string RemoteUserId,
    Guid TrackedGameId    
) : IRequest<Unit>;

public class DeleteTrackedGameValidator : AbstractValidator<DeleteTrackedGameCommand>
{
    public DeleteTrackedGameValidator()
    {
        RuleFor(c => c.TrackedGameId).NotNull();
    }
}

public class DeleteTrackedGameHandler : IRequestHandler<DeleteTrackedGameCommand, Unit>
{
    private readonly DatabaseContext _databaseContext;

    public DeleteTrackedGameHandler(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }
    
    public async Task<Unit> Handle(DeleteTrackedGameCommand command, CancellationToken cancellationToken)
    {
        TrackedGame? trackedGame = await _databaseContext.TrackedGames
            .Where(tg => tg.Id == command.TrackedGameId)
            .FirstOrDefaultAsync(cancellationToken);

        if (trackedGame == null)
        {
            throw new NotFoundException("Tracked game not found!");
        }

        if (trackedGame.UserRemoteId != command.RemoteUserId)
        {
            throw new ForbiddenException();
        }

        _databaseContext.TrackedGames.Remove(trackedGame);
        await _databaseContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}