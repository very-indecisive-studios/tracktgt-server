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
    private readonly IGameService _gameService;
    private readonly IMapper _mapper;

    public AddGameTrackingHandler(DatabaseContext dbContext, IGameService gameService, IMapper mapper)
    {
        _dbContext = dbContext;
        _gameService = gameService;
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
        
        // Verify game id.
        bool isGameExists = await _dbContext.Games
            .AsNoTracking()
            .Where(g => g.RemoteId == command.GameRemoteId)
            .AnyAsync(cancellationToken);
        // Fetch from external API and store in db if game not cached
        if (!isGameExists)
        {
            APIGame? apiGame = await _gameService.GetGameById(command.GameRemoteId);

            if (apiGame == null)
            {
                throw new NotFoundException("Game not found!");
            }

            Game game = _mapper.Map<APIGame, Game>(apiGame);
            _dbContext.Games.Add(game);
        }

        var gameTracking = _mapper.Map<AddGameTrackingCommand, GameTracking>(command);
        _dbContext.GameTrackings.Add(gameTracking);
        
        Activity activity = new Activity();
        activity.UserRemoteId = command.UserRemoteId;
        activity.MediaRemoteId = command.GameRemoteId.ToString();
        activity.MediaStatus = command.Status.ToString();
        activity.NoOf = (int) command.HoursPlayed;
        activity.MediaType = TypeOfMedia.Game;
        activity.Action = ActivityAction.Add;
        _dbContext.Activities.Add(activity);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}
