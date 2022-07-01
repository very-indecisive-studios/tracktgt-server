using AutoMapper;
using Domain.User;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Users.Following;

public record GetUserFollowersQuery(string CurrentUserId) : IRequest<GetUserFollowersResult>;

public class GetUserFollowersValidator : AbstractValidator<GetUserFollowersQuery>
{
    public GetUserFollowersValidator()
    {
        RuleFor(q => q.CurrentUserId).NotEmpty();
    }
}

public record GetUserFollowersResult(List<GetUserFollowersResult.GetUserFollowersItemResult> Items)
{
    public record GetUserFollowersItemResult(
        string UserName,
        string ProfilePictureURL
    );
}

public static class GetUserFollowersMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<User, GetUserFollowersResult.GetUserFollowersItemResult>();
    }
}

public class GetUserFollowersHandler : IRequestHandler<GetUserFollowersQuery, GetUserFollowersResult>
{
    private readonly DatabaseContext _databaseContext;
    private readonly IMapper _mapper;

    public GetUserFollowersHandler(DatabaseContext databaseContext, IMapper mapper)
    {
        _databaseContext = databaseContext;
        _mapper = mapper;
    }
    
    public async Task<GetUserFollowersResult> Handle(GetUserFollowersQuery query, CancellationToken cancellationToken)
    {
        var followers = await _databaseContext.Follows
            .AsNoTracking()
            .Where(f => f.FollowingUserId == query.CurrentUserId)
            .Join(
                _databaseContext.Users,
                f => f.FollowerUserId,
                u => u.RemoteId,
                (f, u) => _mapper.Map<User, GetUserFollowersResult.GetUserFollowersItemResult>(u)
            )
            .ToListAsync(cancellationToken);
        
        return new GetUserFollowersResult(followers);
    }
}