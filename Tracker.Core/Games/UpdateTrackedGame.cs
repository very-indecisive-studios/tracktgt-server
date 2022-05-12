using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tracker.Core.Exceptions;
using Tracker.Domain;
using Tracker.Persistence;
using Tracker.Service.Game;

namespace Tracker.Core.Games;

public record UpdateTrackedGameCommand(
    string UserRemoteId,
    long GameRemoteId,
    float HoursPlayed,
    string Platform,
    GameFormat Format,
    GameStatus Status,
    GameOwnership Ownership
) : IRequest<Unit>;

public class UpdateTrackedGameValidator : AbstractValidator<UpdateTrackedGameCommand>
{
    public UpdateTrackedGameValidator()
    {
        RuleFor(c => c.UserRemoteId).NotEmpty();
        RuleFor(c => c.GameRemoteId).NotEmpty();
        RuleFor(c => c.Platform).NotEmpty();
    }
}

public static class UpdateTrackedGameMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<UpdateTrackedGameCommand, TrackedGame>();
    }
}

public class UpdateTrackedGameHandler : IRequestHandler<UpdateTrackedGameCommand, Unit>
{
    private readonly DatabaseContext _dbContext;
    private readonly IMapper _mapper;

    public UpdateTrackedGameHandler(DatabaseContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }
    
    public async Task<Unit> Handle(UpdateTrackedGameCommand command, CancellationToken cancellationToken)
    {
        TrackedGame? trackedGame = await _dbContext.TrackedGames
            .Where(tg => tg.GameRemoteId == command.GameRemoteId && tg.UserRemoteId == command.UserRemoteId)
            .FirstOrDefaultAsync(cancellationToken);

        if (trackedGame == null)
        {
            throw new NotFoundException();
        }

        _mapper.Map<UpdateTrackedGameCommand, TrackedGame>(command, trackedGame);
        _dbContext.TrackedGames.Update(trackedGame);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}