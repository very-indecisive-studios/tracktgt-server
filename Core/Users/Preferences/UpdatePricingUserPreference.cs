using AutoMapper;
using Domain.User;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Users.Preferences;

public record UpdatePricingUserPreferenceCommand(
    string UserRemoteId,
    string EShopRegion
) : IRequest<Unit>;

public class UpdatePricingUserPreferenceValidator : AbstractValidator<UpdatePricingUserPreferenceCommand>
{
    public UpdatePricingUserPreferenceValidator()
    {
        RuleFor(c => c.UserRemoteId).NotEmpty();
    }
}

public static class UpdatePricingUserPreferenceMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<UpdatePricingUserPreferenceCommand, PricingUserPreference>();
    }
}

public class UpdatePricingUserPreferenceHandler 
    : IRequestHandler<UpdatePricingUserPreferenceCommand, Unit>
{
    private readonly DatabaseContext _databaseContext;
    private readonly IMapper _mapper;

    public UpdatePricingUserPreferenceHandler(DatabaseContext databaseContext, IMapper mapper)
    {
        _databaseContext = databaseContext;
        _mapper = mapper;
    }
    
    public async Task<Unit> Handle(UpdatePricingUserPreferenceCommand command,
        CancellationToken cancellationToken)
    {
        var pricingUserPreferences = await _databaseContext.PricingUserPreferences
            .Where(pup => pup.UserRemoteId == command.UserRemoteId)
            .FirstOrDefaultAsync(cancellationToken);

        if (pricingUserPreferences == null)
        {
            pricingUserPreferences = new PricingUserPreference
            {
                UserRemoteId = command.UserRemoteId,
            };

            pricingUserPreferences = _mapper.Map(command, pricingUserPreferences);

            _databaseContext.Add(pricingUserPreferences);
        }
        else
        {
            pricingUserPreferences = _mapper.Map(command, pricingUserPreferences);

            _databaseContext.Update(pricingUserPreferences);
        }
        
        await _databaseContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}