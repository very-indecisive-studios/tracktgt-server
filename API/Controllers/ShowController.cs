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
    [HttpGet("track/{userRemoteId}/{showRemoteId:int}", Name = nameof(GetShowTracking))]
    public Task<GetShowTrackingResult> GetShowTracking(string userRemoteId, int showRemoteId, ShowType showType)
    {
        return Mediator.Send(new GetShowTrackingQuery(userRemoteId, showRemoteId, showType));
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpGet("series/{id:int}", Name = nameof(GetSeries))]
    public Task<GetShowResult> GetSeries(int id)
    {
        return Mediator.Send(new GetShowQuery(id, ShowType.Series));
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpGet("movie/{id:int}", Name = nameof(GetMovie))]
    public Task<GetShowResult> GetMovie(int id)
    {
        return Mediator.Send(new GetShowQuery(id, ShowType.Movie));
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