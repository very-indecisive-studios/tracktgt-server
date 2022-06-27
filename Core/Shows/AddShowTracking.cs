using AutoMapper;
using Core.Exceptions;
using Domain;
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
    ShowType ShowType,
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
        
        // Fetch from external API and store in db if show do not exist.
        bool isShowExists = await _dbContext.Shows
            .AsNoTracking()
            .Where(show => show.RemoteId == command.ShowRemoteId)
            .AnyAsync(cancellationToken);

        if (!isShowExists)
        {
            APIShow? apiShow = await _showService.GetShowById(command.ShowRemoteId);
            
            if (apiShow == null)
            {
                throw new NotFoundException("Show not found!");
            }
            _dbContext.Shows.Add(_mapper.Map<APIShow, Show>(apiShow));
        }
        
        var showTracking = _mapper.Map<AddShowTrackingCommand, ShowTracking>(command);
        _dbContext.ShowTrackings.Add(showTracking);

        Activity activity = new Activity();
        activity.UserRemoteId = command.UserRemoteId;
        activity.MediaRemoteId = command.ShowRemoteId;
        activity.MediaStatus = command.Status.ToString();
        activity.NoOf = command.EpisodesWatched;
        activity.MediaType = TypeOfMedia.Show;
        activity.Action = ActivityAction.Add;
        _dbContext.Activities.Add(activity);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}