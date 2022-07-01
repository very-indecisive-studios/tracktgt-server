using AutoMapper;
using AutoMapper.QueryableExtensions;
using Core.Exceptions;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Users.Activity;

public record GetUserActivityQuery(string UserRemoteId) : IRequest<GetUserActivityResult>;

public class GetUserActivityValidator : AbstractValidator<GetUserActivityQuery>
{
    public GetUserActivityValidator()
    {
        RuleFor(q => q.UserRemoteId).NotEmpty();
    }
}

public record GetUserActivityResult(List<GetUserActivityResult.GetUserActivityItemResult> Items)
{
    public record GetUserActivityItemResult(
        string MediaRemoteId,
        string MediaStatus,
        int NoOf,
        TypeOfMedia MediaType,
        ActivityAction Action
    );
}

public static class GetUserActivityMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<Domain.Activity, GetUserActivityResult>();
    }
}

public class GetUserActivityHandler : IRequestHandler<GetUserActivityQuery, GetUserActivityResult>
{
    private readonly DatabaseContext _databaseContext;
    private readonly IMapper _mapper;

    public GetUserActivityHandler(DatabaseContext databaseContext, IMapper mapper)
    {
        _databaseContext = databaseContext;
        _mapper = mapper;
    }
    
    public async Task<GetUserActivityResult> Handle(GetUserActivityQuery query, CancellationToken cancellationToken)
    {
        var activities = await _databaseContext.Activities
            .AsNoTracking()
            .Where(a => a.UserRemoteId == query.UserRemoteId)
            .ProjectTo<GetUserActivityResult.GetUserActivityItemResult>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
        
        return new (activities);
    }
}