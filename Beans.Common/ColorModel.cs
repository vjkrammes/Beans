using System.Globalization;

namespace Beans.Common;

public class ColorModel
{
    public string Name { get; init; }
    public byte A { get; init; }
    public byte R { get; init; }
    public byte G { get; init; }
    public byte B { get; init; }
    public long Value { get; private set; }

    public ColorModel(string name, byte a, byte r, byte g, byte b)
    {
        Name = name;
        A = a;
        R = r;
        G = g;
        B = b;
        Value = r << 16 | g << 8 | b;
    }

    public override string ToString() => Name;

    public static ColorModel FromString(string spec)
    {
        if (string.IsNullOrWhiteSpace(spec))
        {
            throw new ArgumentException("Color specification is  required", nameof(spec));
        }
        var parts = spec.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            throw new ArgumentException("Specification is invalid, expected 'name=hexvalue'");
        }
        if (parts[1].Length != 8)
        {
            throw new ArgumentException("Specification is invalid, expected 8-digit hex value for color");
        }
        var a = (byte)int.Parse(parts[1][..2], NumberStyles.HexNumber);
        var r = (byte)int.Parse(parts[1].Substring(2, 2), NumberStyles.HexNumber);
        var g = (byte)int.Parse(parts[1].Substring(4, 2), NumberStyles.HexNumber);
        var b = (byte)int.Parse(parts[1].Substring(6, 2), NumberStyles.HexNumber);
        return new(parts[0], a, r, g, b);
    }
}
