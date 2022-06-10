using System.Net.Mime;
using Core.Books.Content;
using Core.Books.Tracking;
using Core.Books.Wishlist;
using Core.Common;
using MediatR;
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
    [HttpPost("wishlist", Name = nameof(AddBookWishlist))]
    public Task<Unit> AddBookWishlist(AddBookWishlistCommand addBookWishlistCommand)
    {
        return Mediator.Send(addBookWishlistCommand);
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpDelete("wishlist", Name = nameof(RemoveBookWishlist))]
    public Task<Unit> RemoveBookWishlist(RemoveBookWishlistCommand removeBookWishlistCommand)
    {
        return Mediator.Send(removeBookWishlistCommand);
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpGet("wishlist", Name = nameof(GetAllBookWishlists))]
    public Task<PagedListResult<GetAllBookWishlistsItemResult>> GetAllBookWishlists(
        [FromQuery] GetAllBookWishlistsQuery getAllBookWishlistsQuery)
    {
        return Mediator.Send(getAllBookWishlistsQuery);
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpGet("wishlist/{userRemoteId}/{bookRemoteId}", Name = nameof(GetBookWishlist))]
    public Task<bool> GetBookWishlist(string userRemoteId, string bookRemoteId)
    {
        return Mediator.Send(new GetBookWishlistQuery(userRemoteId, bookRemoteId));
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpPost("track", Name = nameof(AddBookTracking))]
    public Task<Unit> AddBookTracking(AddBookTrackingCommand addBookTrackingCommand)
    {
        return Mediator.Send(addBookTrackingCommand);
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpDelete("track", Name = nameof(RemoveBookTracking))]
    public Task<Unit> RemoveBookTracking(RemoveBookTrackingCommand removeBookTrackingCommand)
    {
        return Mediator.Send(removeBookTrackingCommand);
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpPut("track", Name = nameof(UpdateBookTracking))]
    public Task<Unit> UpdateBookTracking(UpdateBookTrackingCommand updateBookTrackingCommand)
    {
        return Mediator.Send(updateBookTrackingCommand);
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpGet("track", Name = nameof(GetAllBookTrackings))]
    public Task<PagedListResult<GetAllBookTrackingsItemResult>> GetAllBookTrackings(
        [FromQuery] GetAllBookTrackingsQuery getAllBookTrackingsQuery)
    {
        return Mediator.Send(getAllBookTrackingsQuery);
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpGet("track/{userRemoteId}/{bookRemoteId}", Name = nameof(GetBookTracking))]
    public Task<GetBookTrackingResult?> GetBookTracking(string userRemoteId, string bookRemoteId)
    {
        return Mediator.Send(new GetBookTrackingQuery(userRemoteId, bookRemoteId));
    }
    
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