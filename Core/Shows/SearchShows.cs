using AutoMapper;
using Domain;
using Domain.Media;
using FluentValidation;
using MediatR;
using Service.Show;

namespace Core.Shows;

public record SearchShowsQuery(string Title) : IRequest<SearchShowsResult>;

public class SearchShowsValidator : AbstractValidator<SearchShowsQuery>
{
    public SearchShowsValidator()
    {
        RuleFor(query => query.Title).NotEmpty();
    }
}

public record SearchShowsResult(List<SearchShowsResult.SearchShowsItemResult> Items)
{
    public record SearchShowsItemResult(string RemoteId, string Title, string CoverImageURL, ShowType ShowType);
}

public static class SearchShowsMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<APIShowBasic, SearchShowsResult.SearchShowsItemResult>()
            .ForCtorParam(
                "RemoteId",
                options => options.MapFrom(apiShow => apiShow.Id));
    }
}

public class SearchShowsHandler : IRequestHandler<SearchShowsQuery, SearchShowsResult>
{
    private readonly IShowService _showService;
    private readonly IMapper _mapper;
    
    public SearchShowsHandler(IShowService showService, IMapper mapper)
    {
        _showService = showService;
        _mapper = mapper;
    }
    
    public async Task<SearchShowsResult> Handle(SearchShowsQuery searchShowsQuery, CancellationToken cancellationToken)
    {
        var shows = await _showService.SearchShowByTitle(searchShowsQuery.Title);

        return new SearchShowsResult(shows.Select(_mapper.Map<APIShowBasic, SearchShowsResult.SearchShowsItemResult>).ToList());
    }
}