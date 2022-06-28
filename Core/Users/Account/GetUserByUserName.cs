using AutoMapper;
using Core.Exceptions;
using Domain.User;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Users.Account;

public record GetUserByUserNameQuery(string UserName) : IRequest<GetUserByUserNameResult>;

public class GetUserByUserNameValidator : AbstractValidator<GetUserByUserNameQuery>
{
    public GetUserByUserNameValidator()
    {
        RuleFor(q => q.UserName).NotEmpty();
    }
}

public record GetUserByUserNameResult(
    string RemoteId,
    string ProfilePictureURL,
    string UserName,
    string Email,
    string Bio
);

public static class GetUserByUserNameMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<User, GetUserByUserNameResult>();
    }
}

public class GetUserByUserNameHandler : IRequestHandler<GetUserByUserNameQuery, GetUserByUserNameResult>
{
    private readonly DatabaseContext _databaseContext;
    private readonly IMapper _mapper;

    public GetUserByUserNameHandler(DatabaseContext databaseContext, IMapper mapper)
    {
        _databaseContext = databaseContext;
        _mapper = mapper;
    }
    
    public async Task<GetUserByUserNameResult> Handle(GetUserByUserNameQuery query, CancellationToken cancellationToken)
    {
        var user = await _databaseContext.Users.AsNoTracking()
            .Where(u => u.UserName == query.UserName)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User not found!");
        }

        return _mapper.Map<GetUserByUserNameResult>(user);
    }
}