using Core.Exceptions;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Books.Wishlist;

public record RemoveBookWishlistCommand(
    string UserRemoteId,
    string BookRemoteId
) : IRequest<Unit>;

public class RemoveBookWishlistValidator : AbstractValidator<RemoveBookWishlistCommand>
{
    public RemoveBookWishlistValidator()
    {
        RuleFor(c => c.UserRemoteId).NotEmpty();
        RuleFor(c => c.BookRemoteId).NotEmpty();
    }
}

public class RemoveBookWishlistHandler : IRequestHandler<RemoveBookWishlistCommand, Unit>
{
    private readonly DatabaseContext _databaseContext;

    public RemoveBookWishlistHandler(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }
    
    public async Task<Unit> Handle(RemoveBookWishlistCommand command, CancellationToken cancellationToken)
    {
        BookWishlist? bookWishlist = await _databaseContext.BookWishlists
            .Where(bw => bw.BookRemoteId.Equals(command.BookRemoteId) 
                         && bw.UserRemoteId.Equals(command.UserRemoteId))
            .FirstOrDefaultAsync(cancellationToken);

        if (bookWishlist == null)
        {
            throw new NotFoundException("Wishlisted book not found!");
        }

        _databaseContext.BookWishlists.Remove(bookWishlist);
        await _databaseContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}