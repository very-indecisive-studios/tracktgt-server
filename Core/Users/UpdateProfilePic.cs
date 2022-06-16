using AutoMapper;
using Core.Exceptions;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Users;

public record UpdateProfilePicCommand(
    string UserRemoteId,
    string ProfilePictureURL
) : IRequest<Unit>;

public class UpdateProfilePicValidator : AbstractValidator<UpdateProfilePicCommand>
{
    public UpdateProfilePicValidator()
    {
        RuleFor(c => c.UserRemoteId).NotEmpty();
        RuleFor(c => c.ProfilePictureURL).NotEmpty();
    }
}

public static class UpdateProfilePicMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<UpdateProfilePicCommand, User>()
            .ForMember(
                user => user.RemoteId,
                options => options.MapFrom(command => command.UserRemoteId));
    }
}

public class UpdateProfilePicHandler : IRequestHandler<UpdateProfilePicCommand, Unit>
{
    private readonly DatabaseContext _dbContext;
    private readonly IMapper _mapper;

    public UpdateProfilePicHandler(DatabaseContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }
    
    public async Task<Unit> Handle(UpdateProfilePicCommand command, CancellationToken cancellationToken)
    {
        User? user = await _dbContext.Users
            .Where(user => user.RemoteId == command.UserRemoteId)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            throw new NotFoundException();
        }

        _mapper.Map<UpdateProfilePicCommand, User>(command, user);
        _dbContext.Users.Update(user);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}