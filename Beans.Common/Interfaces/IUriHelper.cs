namespace Beans.Common.Interfaces;
public interface IUriHelper
{
    void SetBase(string path);
    void SetVersion(int version);
    Uri Create(string controller, string? action = null, params object[] parms);
}
