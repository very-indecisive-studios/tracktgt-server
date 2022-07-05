using AutoMapper;
using Core.Exceptions;
using Domain;
using Domain.Media;
using Domain.Tracking;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Service.Book;

namespace Core.Books.Tracking;

public record AddBookTrackingCommand(
    string UserRemoteId,
    string BookRemoteId,
    int ChaptersRead,
    BookTrackingFormat Format,
    BookTrackingStatus Status,
    BookTrackingOwnership Ownership
) : IRequest<Unit>;

public class AddBookTrackingValidator : AbstractValidator<AddBookTrackingCommand>
{
    public AddBookTrackingValidator()
    {
        RuleFor(c => c.UserRemoteId).NotEmpty();
        RuleFor(c => c.BookRemoteId).NotEmpty();
    }
}

public static class AddBookTrackingMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<AddBookTrackingCommand, BookTracking>();
        
        // APIBook => Book mapping exists in GetBook use case.
    }
}

public class AddBookTrackingHandler : IRequestHandler<AddBookTrackingCommand, Unit>
{
    private readonly DatabaseContext _dbContext;
    private readonly IMapper _mapper;

    public AddBookTrackingHandler(DatabaseContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(AddBookTrackingCommand command, CancellationToken cancellationToken)
    {
        // Verify user.
        bool isUserExists = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.RemoteId == command.UserRemoteId)
            .AnyAsync(cancellationToken);

        if (!isUserExists)
        {
            throw new NotFoundException("User not found!");
        }
        
        // Verify if tracked book already exist.
        bool isBookTrackingExists = await _dbContext.BookTrackings
            .AsNoTracking()
            .Where(bt => bt.BookRemoteId == command.BookRemoteId 
                         && bt.UserRemoteId == command.UserRemoteId)
            .AnyAsync(cancellationToken);

        if (isBookTrackingExists)
        {
            throw new ExistsException("Tracked book already exists!");
        }
        
        var bookTracking = _mapper.Map<AddBookTrackingCommand, BookTracking>(command);
        _dbContext.BookTrackings.Add(bookTracking);
        
        // Verify book id.
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
        activity.Action = ActivityAction.Add;
        _dbContext.Activities.Add(activity);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}
