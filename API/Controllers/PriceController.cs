using System.Net.Mime;
using Core.Pricing;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class PriceController : APIControllerBase
{
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetSwitchGamePriceResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpGet("switch/{region}/{id:long}", Name = nameof(GetSwitchGamePrice))]
    public Task<GetSwitchGamePriceResult?> GetSwitchGamePrice(string region, long id)
    {
        return Mediator.Send(new GetSwitchGamePriceQuery(region, id));
    }
}