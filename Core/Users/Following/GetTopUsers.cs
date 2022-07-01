using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Users.Following;

public record GetTopUsersQuery(int NoOfUsers) : IRequest<GetTopUsersResult>;

public class GetTopUsersValidator : AbstractValidator<GetTopUsersQuery>
{
    public GetTopUsersValidator()
    {
        RuleFor(q => q.NoOfUsers).NotEmpty();
    }
}

public record GetTopUsersResult(
    List<String> TopUsersList
);

public class GetTopUsersHandler : IRequestHandler<GetTopUsersQuery, GetTopUsersResult>
{
    private readonly DatabaseContext _databaseContext;

    public GetTopUsersHandler(DatabaseContext databaseContext, IMapper mapper)
    {
        _databaseContext = databaseContext;
    }
    
    public async Task<GetTopUsersResult> Handle(GetTopUsersQuery query, CancellationToken cancellationToken)
    {
        var listOfFollows = await _databaseContext.Follows
            .AsNoTracking().ToListAsync(cancellationToken);
        
        IDictionary<string, int> userAndFollowers = new Dictionary<string, int>();

        foreach (var f in listOfFollows)
        {
            var currentUser = f.FollowingUserId;
            if (userAndFollowers.ContainsKey(currentUser))
            {
                userAndFollowers[currentUser] += 1;
            }
            else
            {
                userAndFollowers.Add(currentUser, 1);
            }
        }

        List<String> sortedTopUsersList = new List<String>();
        foreach (var userAndFollower in userAndFollowers.OrderBy(key => key.Value))
        {
            sortedTopUsersList.Add(userAndFollower.Key);
        }
        
        List<String> limitedSortedTopUsersList = new List<String>(query.NoOfUsers);
        for (int i = 0; i < query.NoOfUsers; i++)
        {
            limitedSortedTopUsersList.Add(sortedTopUsersList[i]);
        }
        
        return new GetTopUsersResult(limitedSortedTopUsersList);
    }
}