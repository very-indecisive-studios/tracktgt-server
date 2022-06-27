using AutoMapper;
using Core.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Domain;
using Domain.User;
using Persistence;

namespace Core.Users;

public record RegisterUserCommand(
    string UserRemoteId,
    string Email,
    string UserName
) : IRequest<Unit>;

public class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserValidator()
    {
        RuleFor(c => c.UserRemoteId).NotEmpty();
        RuleFor(c => c.Email).NotEmpty();
        RuleFor(c => c.UserName).NotEmpty();
    }
}

public static class RegisterUserMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<RegisterUserCommand, User>()
            .ForMember(
                user => user.RemoteId,
                options => options.MapFrom(command => command.UserRemoteId));
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
        var isRemoteIdExists = await _dbContext.Users
            .AnyAsync(u => u.RemoteId.Equals(command.UserRemoteId), cancellationToken);
        if (isRemoteIdExists)
        {
            throw new ExistsException("Remote id already exists!");
        }
       
        var isUserNameTaken = await _dbContext.Users
            .AnyAsync(u => u.UserName.Equals(command.UserName), cancellationToken);
        if (isUserNameTaken)
        {
            throw new ExistsException("User name already exists!");
        }
        
        var isEmailTaken = await _dbContext.Users
            .AnyAsync(u => u.Email.Equals(command.Email), cancellationToken);
        if (isEmailTaken)
        {
            throw new ExistsException("Email already exists!");
        }
        
        _dbContext.Users.Add(_mapper.Map<RegisterUserCommand, User>(command));
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}
