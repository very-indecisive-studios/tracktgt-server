using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tracker.Core.Exceptions;
using Tracker.Domain;
using Tracker.Persistence;

namespace Tracker.Core.Users;

public record RegisterUserCommand(
    string RemoteUserId,
    string Email,
    string UserName
) : IRequest<Unit>;

public class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserValidator()
    {
        RuleFor(c => c.RemoteUserId).NotEmpty();
        RuleFor(c => c.Email).NotEmpty();
        RuleFor(c => c.UserName).NotEmpty();
    }
}

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Unit>
{
    private readonly DatabaseContext _dbContext;
    private readonly IMapper _mapper;

    public RegisterUserHandler(DatabaseContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }
    
    public async Task<Unit> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var isUserNameTaken = await _dbContext.Users
            .Where(u => u.UserName.Equals(command.UserName))
            .AnyAsync(cancellationToken);
        
        var isEmailTaken = await _dbContext.Users
            .Where(u => u.Email.Equals(command.Email))
            .AnyAsync(cancellationToken);

        if (isUserNameTaken && isEmailTaken)
        {
            throw new ExistsException("User already exists!");
        }
        
        _dbContext.Users.Add(_mapper.Map<RegisterUserCommand, User>(command));
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}
