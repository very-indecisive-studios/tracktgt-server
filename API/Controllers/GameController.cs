using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Core.Common;
using Core.Games.Content;
using Core.Games.Tracking;
using Core.Games.Wishlist;

namespace API.Controllers;

public class GameController : APIControllerBase
{
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpPost("wishlist", Name = nameof(AddGameWishlist))]
    public Task<Unit> AddGameWishlist(AddGameWishlistCommand addGameWishlistCommand)
    {
        return Mediator.Send(addGameWishlistCommand);
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpDelete("wishlist", Name = nameof(RemoveGameWishlist))]
    public Task<Unit> RemoveGameWishlist(RemoveGameWishlistCommand removeGameWishlistCommand)
    {
        return Mediator.Send(removeGameWishlistCommand);
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpGet("wishlist", Name = nameof(GetAllGameWishlists))]
    public Task<PagedListResult<GetAllGameWishlistsItemResult>> GetAllGameWishlists(
        [FromQuery] GetAllGameWishlistsQuery getAllGameWishlistsQuery)
    {
        return Mediator.Send(getAllGameWishlistsQuery);
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpGet("wishlist/{userRemoteId}/{gameRemoteId:long}", Name = nameof(GetGameWishlists))]
    public Task<GetGameWishlistsResult> GetGameWishlists(string userRemoteId, long gameRemoteId)
    {
        return Mediator.Send(new GetGameWishlistsQuery(userRemoteId, gameRemoteId));
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpPost("track", Name = nameof(AddGameTracking))]
    public Task<Unit> AddGameTracking(AddGameTrackingCommand addGameTrackingCommand)
    {
        return Mediator.Send(addGameTrackingCommand);
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpDelete("track", Name = nameof(RemoveGameTracking))]
    public Task<Unit> RemoveGameTracking(RemoveGameTrackingCommand removeGameTrackingCommand)
    {
        return Mediator.Send(removeGameTrackingCommand);
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpPut("track", Name = nameof(UpdateGameTracking))]
    public Task<Unit> UpdateGameTracking(UpdateGameTrackingCommand updateGameTrackingCommand)
    {
        return Mediator.Send(updateGameTrackingCommand);
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpGet("track", Name = nameof(GetAllGameTrackings))]
    public Task<PagedListResult<GetAllGameTrackingsItemResult>> GetAllGameTrackings(
        [FromQuery] GetAllGameTrackingsQuery getAllGameTrackingsQuery)
    {
        return Mediator.Send(getAllGameTrackingsQuery);
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpGet("track/{userRemoteId}/{gameRemoteId:long}", Name = nameof(GetGameTrackings))]
    public Task<GetGameTrackingsResult> GetGameTrackings(string userRemoteId, long gameRemoteId)
    {
        return Mediator.Send(new GetGameTrackingsQuery(userRemoteId, gameRemoteId));
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpGet("{id:long}", Name = nameof(GetGame))]
    public Task<GetGameResult> GetGame(long id)
    {
        return Mediator.Send(new GetGameQuery(id));
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpGet("search", Name = nameof(SearchGames))]
    public Task<SearchGamesResult> SearchGames([FromQuery] string title)
    {
        return Mediator.Send(new SearchGamesQuery(title));
    }
    
}