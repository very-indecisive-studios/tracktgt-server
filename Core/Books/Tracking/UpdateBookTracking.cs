using AutoMapper;
using Core.Exceptions;
using Domain;
using Domain.Tracking;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Books.Tracking;

public record UpdateBookTrackingCommand(
    string UserRemoteId,
    string BookRemoteId,
    int ChaptersRead,
    BookTrackingFormat Format,
    BookTrackingStatus Status,
    BookTrackingOwnership Ownership
) : IRequest<Unit>;

public class UpdateBookTrackingValidator : AbstractValidator<UpdateBookTrackingCommand>
{
    public UpdateBookTrackingValidator()
    {
        RuleFor(c => c.UserRemoteId).NotEmpty();
        RuleFor(c => c.BookRemoteId).NotEmpty();
    }
}

public static class UpdateBookTrackingMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<UpdateBookTrackingCommand, BookTracking>();
    }
}

public class UpdateBookTrackingHandler : IRequestHandler<UpdateBookTrackingCommand, Unit>
{
    private readonly DatabaseContext _dbContext;
    private readonly IMapper _mapper;

    public UpdateBookTrackingHandler(DatabaseContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }
    
    public async Task<Unit> Handle(UpdateBookTrackingCommand command, CancellationToken cancellationToken)
    {
        BookTracking? bookTracking = await _dbContext.BookTrackings
            .Where(bt => bt.BookRemoteId == command.BookRemoteId 
                         && bt.UserRemoteId == command.UserRemoteId)
            .FirstOrDefaultAsync(cancellationToken);

        if (bookTracking == null)
        {
            throw new NotFoundException();
        }

        _mapper.Map<UpdateBookTrackingCommand, BookTracking>(command, bookTracking);
        _dbContext.BookTrackings.Update(bookTracking);
        
        var book = await _dbContext.Books
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
        activity.Action = ActivityAction.Update;
        _dbContext.Activities.Add(activity);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}