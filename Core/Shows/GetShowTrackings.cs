using FluentValidation;
using MediatR;

namespace Core.Shows;

public record GetShowTrackingsQuery() : IRequest<GetShowTrackingsResult>;

public class GetShowTrackingsValidator : AbstractValidator<GetShowTrackingsQuery>
{
    public GetShowTrackingsValidator()
    {

    }
}

public record GetShowTrackingsResult();

public static class GetShowTrackingsMappings
{
    public static void Map(Profile profile)
    {

    }
}

public class GetShowTrackingsHandler : IRequestHandler<GetShowTrackingsQuery, GetShowTrackingsResult>
{
    public async Task<GetShowTrackingsResult> Handle(GetShowTrackingsQuery query, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}