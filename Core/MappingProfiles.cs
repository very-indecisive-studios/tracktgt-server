﻿using AutoMapper;
using Core.Books.Content;
using Core.Books.Tracking;
using Core.Books.Wishlist;
using Core.Games.Content;
using Core.Games.Tracking;
using Core.Games.Wishlist;
using Core.Pricing.Switch;
using Core.Shows;
using Core.Users;
using Core.Users.Preferences;

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
        
        AddGameWishlistMappings.Map(this);
        GetGameWishlistsMappings.Map(this);
        #endregion

        #region Users
        RegisterUserMappings.Map(this);
        GetUserMappings.Map(this);
        UpdateProfilePicMappings.Map(this);
        UpdateBioMappings.Map(this);
        #endregion
        
        #region Activity
        GetUserActivityMappings.Map(this);
        #endregion
        
        #region Follows
        FollowUserMappings.Map(this);
        #endregion
        
        #region Shows
        AddShowTrackingMappings.Map(this);
        GetShowTrackingMappings.Map(this);
        UpdateShowTrackingMappings.Map(this);
        SearchShowsMappings.Map(this);
        GetShowMappings.Map(this);
        #endregion

        #region Books
        AddBookTrackingMappings.Map(this);
        GetBookTrackingMappings.Map(this);
        UpdateBookTrackingMappings.Map(this);
        
        SearchBooksMappings.Map(this);
        GetBookMappings.Map(this);
        
        AddBookWishlistMappings.Map(this);
        GetBookWishlistMappings.Map(this);
        #endregion

        #region Pricing
        GetSwitchGamePriceMappings.Map(this);
        GetPricingUserPreferenceMappings.Map(this);
        UpdatePricingUserPreferenceMappings.Map(this);
        #endregion
    }
}