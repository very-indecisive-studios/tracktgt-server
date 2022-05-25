using AutoMapper;
using Core.Exceptions;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Service.Show;

namespace Core.Shows;

public record GetShowQuery(int RemoteId, ShowType ShowType) : IRequest<GetShowResult>;

public class GetShowValidator : AbstractValidator<GetShowQuery>
{
    public GetShowValidator()
    {
        RuleFor(query => query.RemoteId).NotEmpty();
    }
}

public record GetShowResult(
    int RemoteId,
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
            .AsNoTracking()
            .Where(game => game.RemoteId == getShowQuery.RemoteId)
            .FirstOrDefaultAsync(cancellationToken);
        if (dbShow != null) return _mapper.Map<Show, GetShowResult>(dbShow);

        // Find show from remote (if not in local)
        var remoteShow = await _showService.GetShowById(getShowQuery.RemoteId, getShowQuery.ShowType);
        if (remoteShow != null)
        {
            var newDBShow = _mapper.Map<APIShow, Show>(remoteShow);
            _dbContext.Shows.Add(newDBShow);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return _mapper.Map<Show, GetShowResult>(newDBShow);
        }

        throw new NotFoundException();
    }
}
