using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tracker.Core.Common;
using Tracker.Core.Games;

namespace Tracker.API.Controllers;

public class GameController : APIControllerBase
{
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