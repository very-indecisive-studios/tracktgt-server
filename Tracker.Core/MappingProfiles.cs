using AutoMapper;
using Tracker.Core.Games;
using Tracker.Domain;
using Tracker.Service.Game;

namespace Tracker.Core;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        #region Game
        // GameService
        CreateMap<APIGame, Game>()
            .ForSourceMember(apiGame => apiGame.Id,
                options => options.DoNotValidate())
            .ForMember(game => game.Id,
                options => options.Ignore())
            .ForMember(
                game => game.RemoteId,
                options => options.MapFrom(apiGame => apiGame.Id));
        
        // AddGame
        CreateMap<AddTrackedGame.Command, TrackedGame>();
        
        // SearchGame
        CreateMap<APIGame, SearchGames.Result.SearchGameResult>();

        // GetGame
        CreateMap<Game, GetGame.Result>()
            .ForSourceMember(game => game.Id,
                options => options.DoNotValidate())
            .ForMember(
                result => result.Id,
                options => options.MapFrom(game => game.RemoteId));
        #endregion
    }
}