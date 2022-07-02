using AutoMapper;
using Core.Exceptions;
using Domain;
using Domain.Media;
using Domain.Tracking;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Service.Show;

namespace Core.Shows;

public record AddShowTrackingCommand(
    string UserRemoteId,
    string ShowRemoteId,
    int EpisodesWatched,
    ShowTrackingStatus Status
) : IRequest<Unit>;

public class AddShowTrackingValidator : AbstractValidator<AddShowTrackingCommand>
{
    public AddShowTrackingValidator()
    {
        RuleFor(showTracking => showTracking.UserRemoteId).NotEmpty();
        RuleFor(showTracking => showTracking.ShowRemoteId).NotEmpty();
    }
}

public static class AddShowTrackingMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<AddShowTrackingCommand, ShowTracking>();
    }
}

public class AddShowTrackingHandler : IRequestHandler<AddShowTrackingCommand, Unit>
{
    private readonly DatabaseContext _dbContext;
    private readonly IShowService _showService;
    private readonly IMapper _mapper;
    
    public AddShowTrackingHandler(DatabaseContext dbContext, IShowService showService, IMapper mapper)
    {
        _dbContext = dbContext;
        _showService = showService;
        _mapper = mapper;
    }

    
    public async Task<Unit> Handle(AddShowTrackingCommand command, CancellationToken cancellationToken)
    {
        // Verify user.
        bool isUserExists = await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.RemoteId == command.UserRemoteId)
            .AnyAsync(cancellationToken);

        if (!isUserExists)
        {
            throw new NotFoundException("User not found!");
        }

        // Verify if tracked show already exist.
        bool isShowTrackingExists = await _dbContext.ShowTrackings
            .AsNoTracking()
            .Where(trackedShow => trackedShow.ShowRemoteId == command.ShowRemoteId 
                         && trackedShow.UserRemoteId == command.UserRemoteId)
            .AnyAsync(cancellationToken);

        if (isShowTrackingExists)
        {
            throw new ExistsException("Tracked show already exists!");
        }
        
        var showTracking = _mapper.Map<AddShowTrackingCommand, ShowTracking>(command);
        _dbContext.ShowTrackings.Add(showTracking);
        
        var show = await _dbContext.Shows
            .AsNoTracking()
            .Where(show => show.RemoteId == command.ShowRemoteId)
            .FirstOrDefaultAsync(cancellationToken);
        if (show == null)
        {
            throw new NotFoundException("Show not found!");
        }
        
        Activity activity = new Activity();
        activity.UserRemoteId = showTracking.UserRemoteId;
        activity.Status = showTracking.Status.ToString();
        activity.NoOf = showTracking.EpisodesWatched;
        activity.MediaRemoteId = show.RemoteId;
        activity.MediaTitle = show.Title;
        activity.MediaCoverImageURL = show.CoverImageURL;
        activity.MediaType = ActivityMediaType.Show;
        activity.Action = ActivityAction.Add;
        _dbContext.Activities.Add(activity);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}