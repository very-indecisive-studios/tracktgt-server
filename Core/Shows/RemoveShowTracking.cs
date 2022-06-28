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
        await _databaseContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}