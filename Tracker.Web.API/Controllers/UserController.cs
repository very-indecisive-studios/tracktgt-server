using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracker.Core.Users;

namespace Tracker.API.Controllers;

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
    [HttpGet("checkUserExist",Name = nameof(CheckUserExist))]
    public Task<CheckUserExistResult> CheckUserExist(CheckUserExistQuery query)
    {
        return Mediator.Send(query);
    }
}