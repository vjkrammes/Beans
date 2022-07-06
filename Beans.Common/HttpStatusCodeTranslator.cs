using Beans.Common.Interfaces;

using System.Net;

namespace Beans.Common;
public class HttpStatusCodeTranslator : IHttpStatusCodeTranslator
{
    public string Translate(int code) => code switch
    {
        0 => "No status code was provided",
        400 => "An invalid request was made.",
        401 => "You are not authenticated. Please log in.",
        403 => "You are not authorized to view that content.",
        404 => "The requested page or object was not found.",
        405 => "An incorrect method was used.",
        408 => "The request timed out.",
        429 => "Too many requests have been received from your IP address.",
        500 => "There was an internal server error.",
        _ => $"HTTP status code {code} was returned from the server."
    };

    public string Translate(HttpStatusCode code) => Translate((int)code);
}
