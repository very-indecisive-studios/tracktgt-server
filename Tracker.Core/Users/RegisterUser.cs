using AutoMapper;
using FluentValidation;
using MediatR;
using Tracker.Domain;
using Tracker.Persistence;

namespace Tracker.Core.Users;

public class RegisterUserCommand : IRequest<Unit>
{
    public string RemoteUserId { get; set; }

    public string Email { get; set; }

    public string UserName { get; set; }
}

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
        _dbContext.Users.Add(_mapper.Map<RegisterUserCommand, User>(command));
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}