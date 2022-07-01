using Core.Exceptions;
using Domain;
using Domain.Tracking;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Books.Tracking;

public record RemoveBookTrackingCommand(
    string UserRemoteId,
    string BookRemoteId
) : IRequest<Unit>;

public class RemoveBookTrackingValidator : AbstractValidator<RemoveBookTrackingCommand>
{
    public RemoveBookTrackingValidator()
    {
        RuleFor(c => c.UserRemoteId).NotEmpty();
        RuleFor(c => c.BookRemoteId).NotEmpty();
    }
}

public class RemoveBookTrackingHandler : IRequestHandler<RemoveBookTrackingCommand, Unit>
{
    private readonly DatabaseContext _databaseContext;

    public RemoveBookTrackingHandler(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }
    
    public async Task<Unit> Handle(RemoveBookTrackingCommand command, CancellationToken cancellationToken)
    {
        BookTracking? bookTracking = await _databaseContext.BookTrackings
            .Where(bt => bt.BookRemoteId == command.BookRemoteId 
                         && bt.UserRemoteId == command.UserRemoteId)
            .FirstOrDefaultAsync(cancellationToken);

        if (bookTracking == null)
        {
            throw new NotFoundException("Tracked book not found!");
        }

        _databaseContext.BookTrackings.Remove(bookTracking);
        
        Activity activity = new Activity();
        activity.UserRemoteId = command.UserRemoteId;
        activity.MediaRemoteId = command.BookRemoteId;
        activity.MediaStatus = bookTracking.Status.ToString();
        activity.NoOf = bookTracking.ChaptersRead;
        activity.MediaType = TypeOfMedia.Book;
        activity.Action = ActivityAction.Remove;
        _databaseContext.Activities.Add(activity);
        
        await _databaseContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}