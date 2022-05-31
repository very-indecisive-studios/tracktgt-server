using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Domain;
using Persistence;

namespace Core.Books;

public record GetBookTrackingQuery(
    string UserRemoteId,
    string BookRemoteId
) : IRequest<GetBookTrackingResult>;

public class GetBookTrackingValidator : AbstractValidator<GetBookTrackingQuery>
{
    public GetBookTrackingValidator()
    {
        RuleFor(q => q.UserRemoteId).NotEmpty();
        RuleFor(q => q.BookRemoteId).NotEmpty();
    }    
}

public record GetBookTrackingResult(
    int ChaptersRead,
    BookTrackingFormat Format,
    BookTrackingStatus Status,
    BookTrackingOwnership Ownership
);

public static class GetBookTrackingMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<BookTracking, GetBookTrackingResult>();
    }
}

public class GetBookTrackingHandler : IRequestHandler<GetBookTrackingQuery, GetBookTrackingResult?>
{
    private readonly DatabaseContext _databaseContext;
    private readonly IMapper _mapper;

    public GetBookTrackingHandler(DatabaseContext databaseContext, IMapper mapper)
    {
        _databaseContext = databaseContext;
        _mapper = mapper;
    }
    
    public async Task<GetBookTrackingResult?> Handle(GetBookTrackingQuery query, CancellationToken cancellationToken)
    {
        var bookTracking = await _databaseContext.BookTrackings
            .AsNoTracking()
            .Where(bt => bt.UserRemoteId == query.UserRemoteId && bt.BookRemoteId == query.BookRemoteId)
            .ProjectTo<GetBookTrackingResult>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        return bookTracking;
    }
}
