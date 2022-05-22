using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Core.Users;

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
    [HttpGet("user/{userRemoteId}", Name = nameof(GetUser))]
    public Task<GetUserResult> GetUser(string userRemoteId)
    {
        return Mediator.Send(new GetUserQuery(userRemoteId));
    }
}