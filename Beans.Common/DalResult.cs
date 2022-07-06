
using Beans.Common.Enumerations;

namespace Beans.Common;
public sealed class DalResult
{
    public DalErrorCode ErrorCode { get; set; }
    public Exception? Exception { get; set; }

    public bool Successful => Exception is null && ErrorCode == DalErrorCode.NoError;

    public DalResult(DalErrorCode errorCode = DalErrorCode.NoError, Exception? exception = null)
    {
        ErrorCode = errorCode;
        Exception = exception;
    }

    public string? ErrorMessage => Exception is not null ? Exception.Innermost() : ErrorCode.GetDescriptionFromEnumValue();

    public static DalResult Duplicate(Exception? ex = null) => new(DalErrorCode.Duplicate, ex);
    public static DalResult NotAutorized(Exception? ex = null) => new(DalErrorCode.NotAuthorized, ex);
    public static DalResult NotFound(Exception? ex = null) => new(DalErrorCode.NotFound, ex);
    public static DalResult Success => new();
    public static DalResult FromException(Exception ex) => new(DalErrorCode.Exception, ex);
}
