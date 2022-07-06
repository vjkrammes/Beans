namespace Beans.Common;
public sealed class ApiResult<TSuccess, TFailure>
{
    private bool _success { get; }
    private TSuccess? _successPayload { get; }
    private TFailure? _failurePayload { get; }

    public ApiResult(TSuccess success)
    {
        _success = true;
        _successPayload = success;
        _failurePayload = default;
    }

    public ApiResult(TFailure failure)
    {
        _success = false;
        _successPayload = default;
        _failurePayload = failure;
    }

    public bool IsSuccessResult => _success;

    public TSuccess SuccessPayload
    {
        get
        {
            if (IsSuccessResult)
            {
                return _successPayload!;
            }
            throw new NotSuccessException();
        }
    }

    public TFailure FailurePayload
    {
        get
        {
            if (IsSuccessResult)
            {
                throw new NotFailureException();
            }
            return _failurePayload!;
        }
    }

    public T Transform<T>(Func<TSuccess, T> successMethod, Func<TFailure, T> failureMethod) =>
      IsSuccessResult ? successMethod(_successPayload!) : failureMethod(_failurePayload!);
}
