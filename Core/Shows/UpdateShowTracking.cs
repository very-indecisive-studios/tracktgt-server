﻿using AutoMapper;
using Core.Exceptions;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Shows;

public record UpdateShowTrackingCommand(
    string UserRemoteId,
    int ShowRemoteId, 
    int EpisodesWatched,
    ShowTrackingStatus Status
) : IRequest<Unit>;

public class UpdateShowTrackingValidator : AbstractValidator<UpdateShowTrackingCommand>
{
    public UpdateShowTrackingValidator()
    {
        RuleFor(c => c.UserRemoteId).NotEmpty();
        RuleFor(c => c.ShowRemoteId).NotEmpty();
    }
}

public static class UpdateShowTrackingMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<UpdateShowTrackingCommand, ShowTracking>();
    }
}

public class UpdateShowTrackingHandler : IRequestHandler<UpdateShowTrackingCommand, Unit>
{
    private readonly DatabaseContext _dbContext;
    private readonly IMapper _mapper;

    public UpdateShowTrackingHandler(DatabaseContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }
    
    public async Task<Unit> Handle(UpdateShowTrackingCommand command, CancellationToken cancellationToken)
    {
        ShowTracking? showTracking = await _dbContext.ShowTrackings
            .Where(showTracking => showTracking.ShowRemoteId == command.ShowRemoteId
                                   && showTracking.UserRemoteId == command.UserRemoteId)
            .FirstOrDefaultAsync(cancellationToken);

        if (showTracking == null)
        {
            throw new NotFoundException();
        }

        _mapper.Map<UpdateShowTrackingCommand, ShowTracking>(command, showTracking);
        _dbContext.ShowTrackings.Update(showTracking);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}