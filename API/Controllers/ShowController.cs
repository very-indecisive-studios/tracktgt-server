using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Core.Common;
using Core.Shows;
using Domain;

namespace API.Controllers;

public class ShowController : APIControllerBase
{
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpPost("track", Name = nameof(AddShowTracking))]
    public Task<Unit> AddShowTracking(AddShowTrackingCommand addShowTrackingCommand)
    {
        return Mediator.Send(addShowTrackingCommand);
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpDelete("track", Name = nameof(RemoveShowTracking))]
    public Task<Unit> RemoveShowTracking(RemoveShowTrackingCommand removeShowTrackingCommand)
    {
        return Mediator.Send(removeShowTrackingCommand);
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpPut("track", Name = nameof(UpdateShowTracking))]
    public Task<Unit> UpdateShowTracking(UpdateShowTrackingCommand updateShowTrackingCommand)
    {
        return Mediator.Send(updateShowTrackingCommand);
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpGet("track", Name = nameof(GetAllShowTrackings))]
    public Task<PagedListResult<GetAllShowTrackingsItemResult>> GetAllShowTrackings(
        [FromQuery] GetAllShowTrackingsQuery getAllShowTrackingsQuery)
    {
        return Mediator.Send(getAllShowTrackingsQuery);
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpGet("track/{userRemoteId}/{showRemoteId:int}", Name = nameof(GetShowTrackings))]
    public Task<GetShowTrackingsResult> GetShowTrackings(string userRemoteId, int showRemoteId)
    {
        return Mediator.Send(new GetShowTrackingsQuery(userRemoteId, showRemoteId));
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpGet("{id:int}/{showType:ShowType}", Name = nameof(GetShow))]
    public Task<GetShowResult> GetShow(int id, ShowType showType)
    {
        return Mediator.Send(new GetShowQuery(id, showType));
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpGet("search", Name = nameof(SearchShows))]
    public Task<SearchShowsResult> SearchShows([FromQuery] string title)
    {
        return Mediator.Send(new SearchShowsQuery(title));
    }
}