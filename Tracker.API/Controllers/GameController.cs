using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Mvc;
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
    public Task<Unit> AddTrackedGame(AddTrackedGame.Command command)
    {
        return _mediator.Send(command);
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpGet("{id:long}", Name = nameof(GetGame))]
    public Task<GetGame.Result> GetGame(long id)
    {
        return _mediator.Send(new GetGame.Query { GameId = id});
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpGet("search", Name = nameof(SearchGames))]
    public Task<SearchGames.Result> SearchGames([FromQuery] string title)
    {
        return _mediator.Send(new SearchGames.Query { GameTitle = title});
    }
}