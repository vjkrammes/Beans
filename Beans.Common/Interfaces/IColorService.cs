namespace Beans.Common.Interfaces;
public interface IColorService
{
    IEnumerable<ColorModel> Colors { get; }
    bool Exists(string name);
    ColorModel? this[string key] { get; }
    public string GetHexString(string name);
    public bool IsBright(string name);
    (byte A, byte R, byte G, byte B) GetARGB(string name);
    ulong GetLongARGB(string name, bool aHigh = true);
    bool AddCustomColor(string name, byte a, byte r, byte g, byte b);
    bool AddCustomColor(string name, ulong colorvalue);
}
