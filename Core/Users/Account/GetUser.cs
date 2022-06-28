using AutoMapper;
using Core.Exceptions;
using Domain.User;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Users.Account;

public record GetUserQuery(string UserRemoteId) : IRequest<GetUserResult>;

public class GetUserValidator : AbstractValidator<GetUserQuery>
{
    public GetUserValidator()
    {
        RuleFor(q => q.UserRemoteId).NotEmpty();
    }
}

public record GetUserResult(
    string UserName,
    string Email
);

public static class GetUserMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<User, GetUserResult>();
    }
}

public class GetUserHandler : IRequestHandler<GetUserQuery, GetUserResult>
{
    private readonly DatabaseContext _databaseContext;
    private readonly IMapper _mapper;

    public GetUserHandler(DatabaseContext databaseContext, IMapper mapper)
    {
        _databaseContext = databaseContext;
        _mapper = mapper;
    }
    
    public async Task<GetUserResult> Handle(GetUserQuery query, CancellationToken cancellationToken)
    {
        var user = await _databaseContext.Users.AsNoTracking()
            .Where(u => u.RemoteId == query.UserRemoteId)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User not found!");
        }

        return _mapper.Map<GetUserResult>(user);
    }
}