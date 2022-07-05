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
        
        var book = await _databaseContext.Books
            .AsNoTracking()
            .Where(b => b.RemoteId == command.BookRemoteId)
            .FirstOrDefaultAsync(cancellationToken);
        if (book == null)
        {
            throw new NotFoundException("Book not found!");
        }

        Activity activity = new Activity();
        activity.UserRemoteId = bookTracking.UserRemoteId;
        activity.Status = bookTracking.Status.ToString();
        activity.NoOf = bookTracking.ChaptersRead;
        activity.MediaRemoteId = book.RemoteId;
        activity.MediaTitle = book.Title;
        activity.MediaCoverImageURL = book.CoverImageURL;
        activity.MediaType = ActivityMediaType.Book;
        activity.Action = ActivityAction.Remove;
        _databaseContext.Activities.Add(activity);
        
        await _databaseContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}