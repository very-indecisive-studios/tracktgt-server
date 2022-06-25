using AutoMapper;
using Domain.User;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Users.Preferences;

public record GetPricingUserPreferenceQuery(
    string UserRemoteId
) : IRequest<GetPricingUserPreferenceResult>;

public class GetPricingUserPreferenceValidator : AbstractValidator<GetPricingUserPreferenceQuery>
{
    public GetPricingUserPreferenceValidator()
    {
        RuleFor(q => q.UserRemoteId).NotEmpty();
    }
}

public record GetPricingUserPreferenceResult(
    string EShopRegion
);

public static class GetPricingUserPreferenceMappings
{
    public static void Map(Profile profile)
    {
        profile.CreateMap<PricingUserPreference, GetPricingUserPreferenceResult>();
    }
}

public class GetPricingUserPreferenceHandler 
    : IRequestHandler<GetPricingUserPreferenceQuery, GetPricingUserPreferenceResult>
{
    private readonly DatabaseContext _databaseContext;
    private readonly IMapper _mapper;

    public GetPricingUserPreferenceHandler(DatabaseContext databaseContext, IMapper mapper)
    {
        _databaseContext = databaseContext;
        _mapper = mapper;
    }
    
    public async Task<GetPricingUserPreferenceResult> Handle(GetPricingUserPreferenceQuery query,
        CancellationToken cancellationToken)
    {
        var pricingUserPreferences = await _databaseContext.PricingUserPreferences
            .AsNoTracking()
            .Where(pup => pup.UserRemoteId == query.UserRemoteId)
            .FirstOrDefaultAsync(cancellationToken);

        if (pricingUserPreferences == null)
        {
            pricingUserPreferences = new PricingUserPreference
            {
                UserRemoteId = query.UserRemoteId,
                EShopRegion = "AU"
            };

            _databaseContext.Add(pricingUserPreferences);

            await _databaseContext.SaveChangesAsync(cancellationToken);
        }

        return _mapper.Map<GetPricingUserPreferenceResult>(pricingUserPreferences);
    }
}
