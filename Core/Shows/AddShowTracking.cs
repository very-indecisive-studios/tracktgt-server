using AutoMapper;
using FluentValidation;
using MediatR;

namespace Core.Shows;

public record AddShowTrackingCommand() : IRequest<AddShowTrackingResult>;

public class AddShowTrackingValidator : AbstractValidator<AddShowTrackingCommand>
{
    public AddShowTrackingValidator()
    {

    }
}

public record AddShowTrackingResult();

public static class AddShowTrackingMappings
{
    public static void Map(Profile profile)
    {

    }
}

public class AddShowTrackingHandler : IRequestHandler<AddShowTrackingCommand, AddShowTrackingResult>
{
    public async Task<AddShowTrackingResult> Handle(AddShowTrackingCommand command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}