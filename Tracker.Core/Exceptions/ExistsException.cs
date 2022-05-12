namespace Tracker.Core.Exceptions;

public class ExistsException : Exception
{
    public ExistsException(string? message) : base(message) { }
}