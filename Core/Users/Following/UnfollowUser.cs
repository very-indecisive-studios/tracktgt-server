using AutoMapper;
using Core.Exceptions;
using Domain.User;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Users.Following;

public record UnfollowUserCommand(
    string FollowerUserId,
    string FollowingUserId
) : IRequest<Unit>;

public class UnfollowUserValidator : AbstractValidator<UnfollowUserCommand>
{
    public UnfollowUserValidator()
    {
        RuleFor(q => q.FollowerUserId).NotEmpty();
        RuleFor(q => q.FollowingUserId).NotEmpty();
    }
}

public class UnfollowUserHandler : IRequestHandler<UnfollowUserCommand, Unit>
{
    private readonly DatabaseContext _databaseContext;

    public UnfollowUserHandler(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }
    
    public async Task<Unit> Handle(UnfollowUserCommand command, CancellationToken cancellationToken)
    {
        var follow = await _databaseContext.Follows
            .Where(f => f.FollowerUserId.Equals(command.FollowerUserId)
                        && f.FollowingUserId.Equals(command.FollowingUserId))
            .FirstOrDefaultAsync(cancellationToken);
        if (follow == null)
        {
            throw new NotFoundException("Not following!");
        }

        _databaseContext.Follows.Remove(follow);
        await _databaseContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}