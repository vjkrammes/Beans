namespace Beans.Common;
public sealed class NotFailureException : ApiResultException
{
    public NotFailureException(string? message = null) : base(message ?? "Cannot retrieve a failure payload from a success result") { }
}
