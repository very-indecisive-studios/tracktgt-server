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
        CreateMap<AddTrackedGameCommand, TrackedGame>();
        
        // SearchGame
        CreateMap<APIGame, SearchGamesResult.SearchGameResult>();

        // GetGame
        CreateMap<Game, GetGameResult>()
            .ForSourceMember(game => game.Id,
                options => options.DoNotValidate())
            .ForMember(
                result => result.Id,
                options => options.MapFrom(game => game.RemoteId));
        #endregion
    }
}