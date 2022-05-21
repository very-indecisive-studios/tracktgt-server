using AutoMapper;
using Core.Games;
using Core.Users;
using Domain;
using Service.Game;

namespace Core;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        #region Games
        AddGameTrackingMappings.Map(this);
        GetGameTrackingsMappings.Map(this);
        UpdateGameTrackingMappings.Map(this);
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
        GetUserMappings.Map(this);
        #endregion
    }
}