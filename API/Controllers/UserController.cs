using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Core.Users;
using Core.Users.Preferences;

namespace API.Controllers;

public class UserController : APIControllerBase
{
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpPost("register",Name = nameof(RegisterUser))]
    public Task<Unit> RegisterUser(RegisterUserCommand command)
    {
        return Mediator.Send(command);
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpGet("checkUserExist", Name = nameof(CheckUserExist))]
    public Task<CheckUserExistResult> CheckUserExist([FromQuery] CheckUserExistQuery query)
    {
        return Mediator.Send(query);
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpGet("{userRemoteId}", Name = nameof(GetUser))]
    public Task<GetUserResult> GetUser(string userRemoteId)
    {
        return Mediator.Send(new GetUserQuery(userRemoteId));
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpGet("preferences/pricing/{userRemoteId}", Name = nameof(GetPricingUserPreference))]
    public Task<GetPricingUserPreferenceResult> GetPricingUserPreference(string userRemoteId)
    {
        return Mediator.Send(new GetPricingUserPreferenceQuery(userRemoteId));
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpPut("preferences/pricing", Name = nameof(UpdatePricingUserPreferenceCommand))]
    public Task<Unit> UpdatePricingUserPreferenceCommand(UpdatePricingUserPreferenceCommand command)
    {
        return Mediator.Send(command);
    }
}