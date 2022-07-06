using Beans.Common.Enumerations;

using System.Text;

namespace Beans.Common;

[Serializable]
public sealed class ApiError
{
    public int Code { get; }
    public string Message { get; }
    public string[] Messages { get; }

    public ApiError(int code = 0, string? message = null, string[]? messages = null)
    {
        Code = code;
        Message = message ?? string.Empty;
        Messages = messages ?? Array.Empty<string>();
    }

    public ApiError(int code) : this(code, null, null) { }

    public ApiError(string message) : this(Constants.SEE_MESSAGE, message) { }

    public ApiError(string[] messages) : this(Constants.SEE_MESSAGE, null, messages) { }

    public ApiError(int code, string message) : this(code, message, null) { }

    public ApiError(int code, string[] messages) : this(code, null, messages) { }

    public ApiError(string message, string[] messages) : this(Constants.SEE_MESSAGE, message, messages) { }

    public bool Successful => Code == 0 && string.IsNullOrWhiteSpace(Message) && (Messages is null || !Messages.Any());

    public string[] Errors()
    {
        var ret = new List<string>();
        if (!string.IsNullOrWhiteSpace(Message))
        {
            ret.Add(Message);
        }
        if (Messages is not null && Messages.Any())
        {
            ret.AddRange(Messages);
        }
        return ret.ToArray();
    }

    public string ErrorMessage()
    {
        var messages = Errors();
        if (messages is null || !messages.Any())
        {
            return string.Empty;
        }
        var sb = new StringBuilder();
        messages.ForEach(x => sb.AppendLine(x));
        return sb.ToString().TrimEnd(new char[] { '\r', '\n' });
    }

    public static ApiError FromDalResult(DalResult result) => new((int)result.ErrorCode, result.Exception?.Innermost() ?? string.Empty);

    public static ApiError FromException(Exception ex) => new((int)DalErrorCode.Exception, ex.Innermost() ?? string.Empty);

    public static ApiError Success => new();
}
