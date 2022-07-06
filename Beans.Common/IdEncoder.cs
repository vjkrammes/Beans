
using HashidsNet;

namespace Beans.Common;
public static class IdEncoder
{
    private static readonly string _salt;
    private static readonly IHashids _hasher;

    static IdEncoder()
    {
        _salt = Constants.DefaultIdEncoderSalt;
        _hasher = new Hashids(_salt, 20);
    }

    public static string EncodeId(int id) => _hasher.Encode(id);

    public static int DecodeId(string id)
    {
        try
        {
            return _hasher.Decode(id)?.FirstOrDefault() ?? 0;
        }
        catch
        {
            return 0;
        }
    }
}
