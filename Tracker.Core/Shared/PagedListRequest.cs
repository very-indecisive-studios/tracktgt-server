namespace Tracker.Core.Shared;

/// <summary>
/// List parameters received from client.
/// </summary>
/// <remarks>
/// Requests can be narrowed down with a variety of query string values:
/// ## Query String Parameters
/// - **Page**: An integer value that designates the page of records that should be returned.
/// - **PageSize**: An integer value that designates the number of records returned on the given page that you would like to return. This value is capped by the internal MaxPageSize.
/// - **Sorts**: A comma delimited ordered list of property names to sort by. Adding a `-` before the name switches to descending sorting.
/// - **Filters**: A comma delimited list of fields to filter by formatted as `{Name}{Operator}{Value}` where
///     - {Name} is the name of a filterable property. You can also have multiple names (for OR logic) by enclosing them in brackets and using a pipe delimiter, eg. `(LikeCount|CommentCount)>10` asks if LikeCount or CommentCount is >10
///     - {Operator} is one of the Operators below
///     - {Value} is the value to use for filtering. You can also have multiple values (for OR logic) by using a pipe delimiter, eg.`Title@= new|hot` will return posts with titles that contain the text "new" or "hot"
///
///    | Operator | Meaning                       | Operator  | Meaning                                      |
///    | -------- | ----------------------------- | --------- | -------------------------------------------- |
///    | `==`     | Equals                        |  `!@=`    | Does not Contains                            |
///    | `!=`     | Not equals                    |  `!_=`    | Does not Starts with                         |
///    | `>`      | Greater than                  |  `@=*`    | Case-insensitive string Contains             |
///    | `&lt;`   | Less than                     |  `_=*`    | Case-insensitive string Starts with          |
///    | `>=`     | Greater than or equal to      |  `==*`    | Case-insensitive string Equals               |
///    | `&lt;=`  | Less than or equal to         |  `!=*`    | Case-insensitive string Not equals           |
///    | `@=`     | Contains                      |  `!@=*`   | Case-insensitive string does not Contains    |
///    | `_=`     | Starts with                   |  `!_=*`   | Case-insensitive string does not Starts with |
/// </remarks>
public abstract class PagedListRequest
{
    protected virtual int MaxPageSize { get; } = 20;
    protected virtual int DefaultPageSize { get; set; } = 10;

    public virtual int Page { get; set; } = 1;

    public int PageSize
    {
        get => DefaultPageSize;
        set => DefaultPageSize = value > MaxPageSize ? MaxPageSize : value;
    }
    
    public string? Filters { get; set; }
    
    public string? Sorts { get; set; }
}