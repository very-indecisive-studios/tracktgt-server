using System.Net.Mime;
using Core.Books;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class BookController : APIControllerBase
{
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpGet("{id}", Name = nameof(GetBook))]
    public Task<GetBookResult> GetBook(string id)
    {
        return Mediator.Send(new GetBookQuery(id));
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpGet("search", Name = nameof(SearchBooks))]
    public Task<SearchBooksResult> SearchBooks([FromQuery] string title)
    {
        return Mediator.Send(new SearchBooksQuery(title));
    }
}