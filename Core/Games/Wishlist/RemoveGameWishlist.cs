﻿using Core.Exceptions;
using Domain;
using Domain.Wishlist;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Games.Wishlist;

public record RemoveGameWishlistCommand(
    string UserRemoteId,
    long GameRemoteId,
    string Platform
) : IRequest<Unit>;

public class RemoveGameWishlistValidator : AbstractValidator<RemoveGameWishlistCommand>
{
    public RemoveGameWishlistValidator()
    {
        RuleFor(c => c.UserRemoteId).NotEmpty();
        RuleFor(c => c.Platform).NotEmpty();
    }
}

public class RemoveGameWishlistHandler : IRequestHandler<RemoveGameWishlistCommand, Unit>
{
    private readonly DatabaseContext _databaseContext;

    public RemoveGameWishlistHandler(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }
    
    public async Task<Unit> Handle(RemoveGameWishlistCommand command, CancellationToken cancellationToken)
    {
        GameWishlist? gameWishlist = await _databaseContext.GameWishlists
            .Where(gw => gw.GameRemoteId == command.GameRemoteId 
                         && gw.UserRemoteId.Equals(command.UserRemoteId)
                         && gw.Platform.Equals(command.Platform))
            .FirstOrDefaultAsync(cancellationToken);

        if (gameWishlist == null)
        {
            throw new NotFoundException("Wishlisted game not found!");
        }

        _databaseContext.GameWishlists.Remove(gameWishlist);
        await _databaseContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}