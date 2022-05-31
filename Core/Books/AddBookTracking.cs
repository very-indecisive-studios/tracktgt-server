using AutoMapper;
using Core.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Domain;
using Persistence;
using Service.Book;

namespace Core.Books;

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
    private readonly IBookService _bookService;
    private readonly IMapper _mapper;

    public AddBookTrackingHandler(DatabaseContext dbContext, IBookService bookService, IMapper mapper)
    {
        _dbContext = dbContext;
        _bookService = bookService;
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
        
        // Verify book id.
        bool isBookExists = await _dbContext.Books
            .AsNoTracking()
            .Where(b => b.RemoteId == command.BookRemoteId)
            .AnyAsync(cancellationToken);
        // Fetch from external API and store in db if book not cached
        if (!isBookExists)
        {
            APIBook? apiBook = await _bookService.GetBookById(command.BookRemoteId);

            if (apiBook == null)
            {
                throw new NotFoundException("Book not found!");
            }

            Book book = _mapper.Map<APIBook, Book>(apiBook);
            _dbContext.Books.Add(book);
        }

        var bookTracking = _mapper.Map<AddBookTrackingCommand, BookTracking>(command);
        _dbContext.BookTrackings.Add(bookTracking);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}
