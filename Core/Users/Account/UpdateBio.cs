using AutoMapper;
using Core.Exceptions;
using Domain.User;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Users.Account;

public record UpdateBioCommand(
    string UserRemoteId,
    string Bio
) : IRequest<Unit>;

public class UpdateBioValidator : AbstractValidator<UpdateBioCommand>
{
    public UpdateBioValidator()
    {
        RuleFor(c => c.UserRemoteId).NotEmpty();
        RuleFor(c => c.Bio).NotEmpty();
    }
}

public static class UpdateBioMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<UpdateBioCommand, User>()
            .ForMember(
                user => user.RemoteId,
                options => options.MapFrom(command => command.UserRemoteId));
    }
}

public class UpdateBioHandler : IRequestHandler<UpdateBioCommand, Unit>
{
    private readonly DatabaseContext _dbContext;
    private readonly IMapper _mapper;

    public UpdateBioHandler(DatabaseContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }
    
    public async Task<Unit> Handle(UpdateBioCommand command, CancellationToken cancellationToken)
    {
        User? user = await _dbContext.Users
            .Where(user => user.RemoteId == command.UserRemoteId)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            throw new NotFoundException();
        }

        _mapper.Map<UpdateBioCommand, User>(command, user);
        _dbContext.Users.Update(user);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}