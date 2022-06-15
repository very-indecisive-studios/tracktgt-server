using AutoMapper;
using Core.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Domain;
using Persistence;

namespace Core.Users;

public record FollowUserCommand(
    string FollowerUserId,
    string FollowingUserId
) : IRequest<Unit>;

public class FollowUserValidator : AbstractValidator<FollowUserCommand>
{
    public FollowUserValidator()
    {
        RuleFor(c => c.FollowerUserId).NotEmpty();
        RuleFor(c => c.FollowingUserId).NotEmpty();
    }
}

public static class FollowUserMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<FollowUserCommand, Follow>();
    }
}

public class FollowUserHandler : IRequestHandler<FollowUserCommand, Unit>
{
    private readonly DatabaseContext _dbContext;
    private readonly IMapper _mapper;

    public FollowUserHandler(DatabaseContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }
    
    public async Task<Unit> Handle(FollowUserCommand command, CancellationToken cancellationToken)
    {
        var isRelationshipExists = await _dbContext.Follows
            .Where(f => f.FollowerUserId == command.FollowerUserId)
            .AnyAsync(f => f.FollowingUserId.Equals(command.FollowingUserId), cancellationToken);
        if (isRelationshipExists)
        {
            throw new ExistsException("Already following!");
        }

        _dbContext.Users.Add(_mapper.Map<FollowUserCommand, User>(command));
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
