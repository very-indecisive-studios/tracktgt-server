using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.User;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Users;

public record SearchUsersQuery(string UserName) : IRequest<SearchUsersResult>;

public class SearchUsersValidator : AbstractValidator<SearchUsersQuery>
{
    public SearchUsersValidator()
    {
        RuleFor(query => query.UserName).NotEmpty();
    }
}

public record SearchUsersResult(List<SearchUsersResult.SearchUsersItemResult> Items)
{
    public record SearchUsersItemResult(string UserName, string ProfilePictureURL, string Bio);
}

public static class SearchUsersMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<User, SearchUsersResult.SearchUsersItemResult>();
    }
}

public class SearchUsersHandler : IRequestHandler<SearchUsersQuery, SearchUsersResult>
{
    private readonly DatabaseContext _databaseContext;
    private readonly IMapper _mapper;
    
    public SearchUsersHandler(DatabaseContext databaseContext, IMapper mapper)
    {
        _databaseContext = databaseContext;
        _mapper = mapper;
    }
    
    public async Task<SearchUsersResult> Handle(SearchUsersQuery query, CancellationToken cancellationToken)
    {
        var searchUsersItemResults = await _databaseContext.Users
            .AsNoTracking()
            .Where(u => u.UserName.Contains(query.UserName))
            .Take(10)
            .ProjectTo<SearchUsersResult.SearchUsersItemResult>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new SearchUsersResult(searchUsersItemResults);
    }
}