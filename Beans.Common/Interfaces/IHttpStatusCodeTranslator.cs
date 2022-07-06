using System.Net;

namespace Beans.Common.Interfaces;
public interface IHttpStatusCodeTranslator
{
    string Translate(int code);
    string Translate(HttpStatusCode code);
}
