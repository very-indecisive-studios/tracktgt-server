using FluentValidation;
using MediatR;
using Tracker.Domain;

namespace Tracker.Core.Games;

public class GetTrackedGames
{
    public class Query : IRequest<Result>
    {
        public Guid UserId { get; set; }
    }

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(q => q.UserId).NotEmpty();
        }
    }

    public class Result
    {
        public class TrackedGameResult
        {
            public long GameId { get; set; }
            
            public string? GameTitle { get; set; }
            
            public float HoursPlayed { get; set; }
    
            public string? Platform { get; set; }
    
            public GameFormat Format { get; set; }

            public GameStatus Status { get; set; }
    
            public GameOwnership Ownership { get; set; }
        }

        public List<TrackedGameResult>? Games { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result>
    {
        public async Task<Result> Handle(Query query, CancellationToken cancellationToken)
        {
            await Task.Delay(1, cancellationToken);
            throw new NotImplementedException();
        }
    }
}