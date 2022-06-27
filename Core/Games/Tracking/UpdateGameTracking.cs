﻿using AutoMapper;
using Core.Exceptions;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Games.Tracking;

public record UpdateGameTrackingCommand(
    string UserRemoteId,
    long GameRemoteId,
    string Platform,
    float HoursPlayed,
    GameTrackingFormat Format,
    GameTrackingStatus Status,
    GameTrackingOwnership Ownership
) : IRequest<Unit>;

public class UpdateGameTrackingValidator : AbstractValidator<UpdateGameTrackingCommand>
{
    public UpdateGameTrackingValidator()
    {
        RuleFor(c => c.UserRemoteId).NotEmpty();
        RuleFor(c => c.GameRemoteId).NotEmpty();
        RuleFor(c => c.Platform).NotEmpty();
    }
}

public static class UpdateGameTrackingMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<UpdateGameTrackingCommand, GameTracking>();
    }
}

public class UpdateGameTrackingHandler : IRequestHandler<UpdateGameTrackingCommand, Unit>
{
    private readonly DatabaseContext _dbContext;
    private readonly IMapper _mapper;

    public UpdateGameTrackingHandler(DatabaseContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }
    
    public async Task<Unit> Handle(UpdateGameTrackingCommand command, CancellationToken cancellationToken)
    {
        GameTracking? gameTracking = await _dbContext.GameTrackings
            .Where(tg => tg.GameRemoteId == command.GameRemoteId 
                         && tg.UserRemoteId == command.UserRemoteId
                         && tg.Platform.Equals(command.Platform))
            .FirstOrDefaultAsync(cancellationToken);

        if (gameTracking == null)
        {
            throw new NotFoundException();
        }

        _mapper.Map<UpdateGameTrackingCommand, GameTracking>(command, gameTracking);
        _dbContext.GameTrackings.Update(gameTracking);
        
        Activity activity = new Activity();
        activity.UserRemoteId = command.UserRemoteId;
        activity.MediaRemoteId = command.GameRemoteId.ToString();
        activity.MediaStatus = command.Status.ToString();
        activity.NoOf = (int) command.HoursPlayed;
        activity.MediaType = TypeOfMedia.Game;
        activity.Action = ActivityAction.Update;
        _dbContext.Activities.Add(activity);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}