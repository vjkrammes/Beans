namespace Beans.Common;
public sealed class NotSuccessException : ApiResultException
{
    public NotSuccessException(string? message = null) : base(message ?? "Cannot retrieve a success payload from a failure result") { }
}
