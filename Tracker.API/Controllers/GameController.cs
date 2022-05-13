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
    [HttpPost("track/add", Name = nameof(AddTrackedGame))]
    public Task<Unit> AddTrackedGame(AddTrackedGameCommand addTrackedGameCommand)
    {
        return Mediator.Send(addTrackedGameCommand);
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpDelete("track/remove", Name = nameof(RemoveTrackedGame))]
    public Task<Unit> RemoveTrackedGame(RemoveTrackedGameCommand removeTrackedGameCommand)
    {
        return Mediator.Send(removeTrackedGameCommand);
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpPut("track/update", Name = nameof(UpdateTrackedGame))]
    public Task<Unit> UpdateTrackedGame(UpdateTrackedGameCommand updateTrackedGameCommand)
    {
        return Mediator.Send(updateTrackedGameCommand);
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpGet("track/get", Name = nameof(GetTrackedGames))]
    public Task<PagedListResult<GetTrackedGamesItemResult>> GetTrackedGames([FromQuery] 
        GetTrackedGamesQuery getTrackedGamesQuery)
    {
        return Mediator.Send(getTrackedGamesQuery);
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpGet("track/get/{userRemoteId}/{gameRemoteId:long}", Name = nameof(GetTrackedGame))]
    public Task<GetTrackedGameResult> GetTrackedGame(string userRemoteId, long gameRemoteId)
    {
        return Mediator.Send(new GetTrackedGameQuery(userRemoteId, gameRemoteId));
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