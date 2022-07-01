﻿using System.Net.Mime;
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
    
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CheckUserExistResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpGet("exist", Name = nameof(CheckUserExist))]
    public Task<CheckUserExistResult> CheckUserExist([FromQuery] CheckUserExistQuery query)
    {
        return Mediator.Send(query);
    }
    
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpGet("id/{userRemoteId}", Name = nameof(GetUser))]
    public Task<GetUserResult> GetUser(string userRemoteId)
    {
        return Mediator.Send(new GetUserQuery(userRemoteId));
    }

    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserByUserNameResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpGet("username/{userName}", Name = nameof(GetUserByUserName))]
    public Task<GetUserByUserNameResult> GetUserByUserName(string userName)
    {
        return Mediator.Send(new GetUserByUserNameQuery(userName));
    }
    
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserStatsResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpGet("stats/{userId}", Name = nameof(GetUserStats))]
    public Task<GetUserStatsResult> GetUserStats(string userId)
    {
        return Mediator.Send(new GetUserStatsQuery(userId));
    }

    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetPricingUserPreferenceResult))]
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