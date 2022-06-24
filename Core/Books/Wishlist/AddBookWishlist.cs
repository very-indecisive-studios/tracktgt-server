using AutoMapper;
using Core.Exceptions;
using Domain;
using Domain.Media;
using Domain.Wishlist;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Service.Book;

namespace Core.Books.Wishlist;

public record AddBookWishlistCommand(
    string UserRemoteId,
    string BookRemoteId
) : IRequest<Unit>;

public class AddBookWishlistValidator : AbstractValidator<AddBookWishlistCommand>
{
    public AddBookWishlistValidator()
    {
        RuleFor(c => c.UserRemoteId).NotEmpty();
        RuleFor(c => c.BookRemoteId).NotEmpty();
    }
}

public static class AddBookWishlistMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<AddBookWishlistCommand, BookWishlist>();
        
        // APIBook => Book mapping exists in GetBook use case.
    }
}

public class AddBookWishlistHandler : IRequestHandler<AddBookWishlistCommand, Unit>
{
    private readonly DatabaseContext _dbContext;
    private readonly IBookService _bookService;
    private readonly IMapper _mapper;

    public AddBookWishlistHandler(DatabaseContext dbContext, IBookService bookService, IMapper mapper)
    {
        _dbContext = dbContext;
        _bookService = bookService;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(AddBookWishlistCommand command, CancellationToken cancellationToken)
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
        bool isBookWishlistExists = await _dbContext.BookWishlists
            .AsNoTracking()
            .Where(bw => bw.BookRemoteId.Equals(command.BookRemoteId) 
                         && bw.UserRemoteId.Equals(command.UserRemoteId))
            .AnyAsync(cancellationToken);

        if (isBookWishlistExists)
        {
            throw new ExistsException("Wishlisted book already exists!");
        }
        
        // Verify book id.
        bool isBookExists = await _dbContext.Books
            .AsNoTracking()
            .Where(b => b.RemoteId.Equals(command.BookRemoteId))
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

        var bookWishlist = _mapper.Map<AddBookWishlistCommand, BookWishlist>(command);
        _dbContext.BookWishlists.Add(bookWishlist);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}
