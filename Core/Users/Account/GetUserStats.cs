using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Users.Account;

public record GetUserStatsQuery(string UserRemoteId) : IRequest<GetUserStatsResult>;

public class GetUserStatsValidator : AbstractValidator<GetUserStatsQuery>
{
    public GetUserStatsValidator()
    {
        RuleFor(q => q.UserRemoteId).NotEmpty();
    }
}

public record GetUserStatsResult(
    float GamingHours,
    int EpisodesWatched,
    int ChaptersRead,
    int Following,
    int Followers
);

public class GetUserStatsHandler : IRequestHandler<GetUserStatsQuery, GetUserStatsResult>
{
    private readonly DatabaseContext _databaseContext;

    public GetUserStatsHandler(DatabaseContext databaseContext, IMapper mapper)
    {
        _databaseContext = databaseContext;
    }
    
    public async Task<GetUserStatsResult> Handle(GetUserStatsQuery query, CancellationToken cancellationToken)
    {
        var gamingHours = await _databaseContext.GameTrackings
            .AsNoTracking()
            .Where(gt => gt.UserRemoteId == query.UserRemoteId)
            .SumAsync(gt => gt.HoursPlayed, cancellationToken);

        var episodesWatched = await _databaseContext.ShowTrackings
            .AsNoTracking()
            .Where(st => st.UserRemoteId == query.UserRemoteId)
            .SumAsync(st => st.EpisodesWatched, cancellationToken);
        
        var chaptersRead = await _databaseContext.BookTrackings
            .AsNoTracking()
            .Where(bt => bt.UserRemoteId == query.UserRemoteId)
            .SumAsync(bt => bt.ChaptersRead, cancellationToken);
        
        var following = await _databaseContext.Follows
            .AsNoTracking()
            .Where(f => f.FollowerUserId == query.UserRemoteId).CountAsync(cancellationToken);
        
        var followers = await _databaseContext.Follows
            .AsNoTracking()
            .Where(f => f.FollowingUserId == query.UserRemoteId).CountAsync(cancellationToken);

        return new GetUserStatsResult(gamingHours,episodesWatched,chaptersRead,following,followers);
    }
}