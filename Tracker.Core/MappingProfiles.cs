using AutoMapper;
using Tracker.Core.Games;
using Tracker.Core.Users;
using Tracker.Domain;
using Tracker.Service.Game;

namespace Tracker.Core;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        #region Games
        // GameService
        CreateMap<APIGame, Game>()
            .ForSourceMember(apiGame => apiGame.Id,
                options => options.DoNotValidate())
            .ForMember(game => game.Id,
                options => options.Ignore())
            .ForMember(
                game => game.RemoteId,
                options => options.MapFrom(apiGame => apiGame.Id))
            .ForMember(
                game => game.PlatformsString,
                options => options.MapFrom(apiGame => string.Join(";", apiGame.Platforms))
            )
            .ForMember(
                game => game.CompaniesString,
                options => options.MapFrom(apiGame => string.Join(";", apiGame.Companies))
            );
        
        AddTrackedGameMappings.Map(this);
        SearchGamesMappings.Map(this);
        GetGameMappings.Map(this);
        #endregion

        #region Users
        // RegisterUser
        CreateMap<RegisterUserCommand, User>()
            .ForSourceMember(command => command.RemoteUserId, 
                    options => options.DoNotValidate())
            .ForMember(
                game => game.RemoteId,
                options => options.MapFrom(command => command.RemoteUserId));
        
        #endregion
    }
}