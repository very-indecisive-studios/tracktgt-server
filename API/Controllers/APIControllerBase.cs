using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class APIControllerBase : ControllerBase
{
    protected IMediator Mediator => HttpContext.RequestServices.GetService<IMediator>() ?? throw new NullReferenceException();
}
