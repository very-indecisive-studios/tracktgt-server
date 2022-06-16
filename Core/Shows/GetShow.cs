using AutoMapper;
using Core.Exceptions;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Service.Show;

namespace Core.Shows;

public record GetShowQuery(string RemoteId) : IRequest<GetShowResult>;

public class GetShowValidator : AbstractValidator<GetShowQuery>
{
    public GetShowValidator()
    {
        RuleFor(query => query.RemoteId).NotEmpty();
    }
}

public record GetShowResult(
    string RemoteId,
    string CoverImageURL,
    string Title,
    string Summary,
    ShowType ShowType
);

public static class GetShowMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<APIShow, Show>()
            .ForMember(show => show.Id,
                options => options.Ignore())
            .ForMember(
                show => show.RemoteId,
                options => options.MapFrom(apiShow => apiShow.Id));
        
        profile.CreateMap<Show, GetShowResult>();
    }
}

public class GetShowHandler : IRequestHandler<GetShowQuery, GetShowResult>
{
    private readonly DatabaseContext _dbContext;
    private readonly IShowService _showService;
    private readonly IMapper _mapper;
    
    public GetShowHandler(DatabaseContext dbContext, IShowService showService, IMapper mapper)
    {
        _dbContext = dbContext;
        _showService = showService;
        _mapper = mapper;
    }
    
    public async Task<GetShowResult> Handle(GetShowQuery getShowQuery, CancellationToken cancellationToken)
    {
        // Find show from local database
        var dbShow = await _dbContext.Shows
            .Where(show => show.RemoteId == getShowQuery.RemoteId)
            .FirstOrDefaultAsync(cancellationToken);

        // Return cached show if its fresh.
        var timeSpan = DateTime.Now - dbShow?.LastModifiedOn;
        if (timeSpan?.TotalHours < 12 && dbShow != null)
        {
             return _mapper.Map<Show, GetShowResult>(dbShow);
        }
        
        // Find show from remote (if not in local)
        var remoteShow = await _showService.GetShowById(getShowQuery.RemoteId);
        if (remoteShow != null)
        {
            if (dbShow == null)
            {
                var newDBShow = _mapper.Map<APIShow, Show>(remoteShow);
                _dbContext.Shows.Add(newDBShow);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return _mapper.Map<Show, GetShowResult>(newDBShow);
            }
            else
            {
                _mapper.Map<APIShow, Show>(remoteShow, dbShow);
                _dbContext.Shows.Update(dbShow);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return _mapper.Map<Show, GetShowResult>(dbShow);
            }
        }

        throw new NotFoundException();
    }
}
