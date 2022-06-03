﻿using AutoMapper;
using Core.Books;
using Core.Games;
using Core.Shows;
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
        RegisterUserMappings.Map(this);
        GetUserMappings.Map(this);
        #endregion
        
        #region Shows
        AddShowTrackingMappings.Map(this);
        GetShowTrackingMappings.Map(this);
        UpdateShowTrackingMappings.Map(this);
        SearchShowsMappings.Map(this);
        GetShowMappings.Map(this);

        #region Books
        SearchBooksMappings.Map(this);
        GetBookMappings.Map(this);
        #endregion
    }
}