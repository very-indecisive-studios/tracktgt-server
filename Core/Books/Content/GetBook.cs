using AutoMapper;
using Core.Exceptions;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Service.Book;

namespace Core.Books.Content;

public record GetBookQuery(string RemoteId) : IRequest<GetBookResult>;

public class GetBookValidator : AbstractValidator<GetBookQuery>
{
    public GetBookValidator()
    {
        RuleFor(query => query.RemoteId).NotEmpty();
    }
}

public record GetBookResult(
    string RemoteId,
    string CoverImageURL,
    string Title,
    string Summary,
    List<string> Authors
);

public static class GetBookMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<APIBook, Book>()
            .ForMember(
                book => book.Id,
                options => options.Ignore())
            .ForMember(
                book => book.RemoteId,
                options => options.MapFrom(apiBook => apiBook.Id))
            .ForMember(
                book => book.AuthorsString,
                options => options.MapFrom(apiBook => string.Join(";", apiBook.Authors)));

        profile.CreateMap<Book, GetBookResult>()
            .ForCtorParam(
                "Authors",
                options => options.MapFrom(book => book.AuthorsString.Split(';', StringSplitOptions.None)));
    }
}

public class GetBookHandler : IRequestHandler<GetBookQuery, GetBookResult>
{
    private readonly DatabaseContext _dbContext;
    private readonly IBookService _bookService;
    private readonly IMapper _mapper;

    public GetBookHandler(DatabaseContext dbContext, IBookService bookService, IMapper mapper)
    {
        _dbContext = dbContext;
        _bookService = bookService;
        _mapper = mapper;
    }

    public async Task<GetBookResult> Handle(GetBookQuery getBookQuery, CancellationToken cancellationToken)
    {
        // Find book from database (cached locally).
        var dbBook = await _dbContext.Books
            .Where(book => book.RemoteId == getBookQuery.RemoteId)
            .FirstOrDefaultAsync(cancellationToken);
        
        var timeSpan = DateTime.Now - dbBook?.LastModifiedOn;
        if (timeSpan?.TotalHours < 12 && dbBook != null)
        {
            return _mapper.Map<Book, GetBookResult>(dbBook);
        }

        // Find book from remote if not cached.
        var remoteBook = await _bookService.GetBookById(getBookQuery.RemoteId);
        if (remoteBook != null)
        {
            if (dbBook == null)
            {
                var newDbBook = _mapper.Map<APIBook, Book>(remoteBook);
                _dbContext.Books.Add(newDbBook);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return _mapper.Map<Book, GetBookResult>(newDbBook);
            }
            else
            {
                _mapper.Map<APIBook, Book>(remoteBook, dbBook);
                _dbContext.Books.Update(dbBook);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return _mapper.Map<Book, GetBookResult>(dbBook);
            }
        }

        throw new NotFoundException();
    }
}