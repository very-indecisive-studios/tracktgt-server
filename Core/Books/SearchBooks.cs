using AutoMapper;
using FluentValidation;
using MediatR;
using Service.Book;

namespace Core.Books;

public record SearchBooksQuery(string Title) : IRequest<SearchBooksResult>;

public class SearchBooksValidator : AbstractValidator<SearchBooksQuery>
{
    public SearchBooksValidator()
    {
        RuleFor(query => query.Title).NotEmpty();
    }
}

public record SearchBooksResult(List<SearchBooksResult.SearchBooksItemResult> Items)
{
    public record SearchBooksItemResult(
        string RemoteId, 
        string Title, 
        string CoverImageURL, 
        List<string> Authors
    );
}

public static class SearchBooksMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<APIBookBasic, SearchBooksResult.SearchBooksItemResult>()
            .ForCtorParam(
                "RemoteId",
                options => options.MapFrom(apiBook => apiBook.Id));
    }
}

public class SearchBooksHandler : IRequestHandler<SearchBooksQuery, SearchBooksResult>
{
    private readonly IBookService _bookService;
    private readonly IMapper _mapper;

    public SearchBooksHandler(IBookService bookService, IMapper mapper)
    {
        _bookService = bookService;
        _mapper = mapper;
    }
    
    public async Task<SearchBooksResult> Handle(SearchBooksQuery searchBooksQuery, CancellationToken cancellationToken)
    {
        var books = await _bookService.SearchBookByTitle(searchBooksQuery.Title);

        return new SearchBooksResult(books.Select(_mapper.Map<APIBookBasic, SearchBooksResult.SearchBooksItemResult>).ToList());
    }
}
