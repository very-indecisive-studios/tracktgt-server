using AutoMapper;
using Core.Exceptions;
using Domain;
using Domain.Media;
using Domain.Tracking;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Service.Game;

namespace Core.Games.Tracking;

public record AddGameTrackingCommand(
    string UserRemoteId,
    long GameRemoteId,
    float HoursPlayed,
    string Platform,
    GameTrackingFormat Format,
    GameTrackingStatus Status,
    GameTrackingOwnership Ownership
) : IRequest<Unit>;

public class AddGameTrackingValidator : AbstractValidator<AddGameTrackingCommand>
{
    public AddGameTrackingValidator()
    {
        RuleFor(c => c.UserRemoteId).NotEmpty();
        RuleFor(c => c.GameRemoteId).NotEmpty();
        RuleFor(c => c.Platform).NotEmpty();
    }
}

public static class AddGameTrackingMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<AddGameTrackingCommand, GameTracking>();
        
        // APIGame => Game mapping exists in GetGame use case.
    }
}

public class AddGameTrackingHandler : IRequestHandler<AddGameTrackingCommand, Unit>
{
    private readonly DatabaseContext _dbContext;
    private readonly IMapper _mapper;

    public AddGameTrackingHandler(DatabaseContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(AddGameTrackingCommand command, CancellationToken cancellationToken)
    {
        // Verify user.
        bool isUserExists = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.RemoteId == command.UserRemoteId)
            .AnyAsync(cancellationToken);

        if (!isUserExists)
        {
            throw new NotFoundException("User not found!");
        }
        
        // Verify if tracked game already exist.
        bool isGameTrackingExists = await _dbContext.GameTrackings
            .AsNoTracking()
            .Where(tg => tg.GameRemoteId == command.GameRemoteId 
                         && tg.UserRemoteId == command.UserRemoteId
                         && tg.Platform.Equals(command.Platform))
            .AnyAsync(cancellationToken);
        if (isGameTrackingExists)
        {
            throw new ExistsException("Tracked game already exists!");
        }
        
        var gameTracking = _mapper.Map<AddGameTrackingCommand, GameTracking>(command);
        _dbContext.GameTrackings.Add(gameTracking);
        
        // Verify game id.
        var game = await _dbContext.Games
            .AsNoTracking()
            .Where(g => g.RemoteId == command.GameRemoteId)
            .FirstOrDefaultAsync(cancellationToken);
        if (game == null)
        {
            throw new NotFoundException("Game not found!");
        }
        
        Activity activity = new Activity();
        activity.UserRemoteId = gameTracking.UserRemoteId;
        activity.Status = gameTracking.Status.ToString();
        activity.NoOf = (int) gameTracking.HoursPlayed;
        activity.MediaRemoteId = game.RemoteId.ToString();
        activity.MediaTitle = game.Title;
        activity.MediaCoverImageURL = game.CoverImageURL;
        activity.MediaType = ActivityMediaType.Game;
        activity.Action = ActivityAction.Add;
        _dbContext.Activities.Add(activity);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}
