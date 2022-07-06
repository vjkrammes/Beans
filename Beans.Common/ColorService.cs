
using Beans.Common.Interfaces;

using Microsoft.Extensions.Configuration;

namespace Beans.Common;
public class ColorService : IColorService
{
    private readonly Dictionary<string, ColorModel> _colors = new(StringComparer.OrdinalIgnoreCase);

    #region Color sources

    private static readonly string[] _colorNames =
    {
            "AliceBlue=fff0f8ff",
            "AntiqueWhite=fffaebd7",
            "Aqua=ff00ffff",
            "Aquamarine=ff7fffd4",
            "Azure=fff0ffff",
            "Beige=fff5f5dc",
            "Bisque=ffffe4c4",
            "Black=ff000000",
            "BlanchedAlmond=ffffebcd",
            "Blue=ff0000ff",
            "BlueViolet=ff8a2be2",
            "Brown=ffa52a2a",
            "BurlyWood=ffdeb887",
            "CadetBlue=ff5f9ea0",
            "Chartreuse=ff7fff00",
            "Chocolate=ffd2691e",
            "Coral=ffff7f50",
            "CornflowerBlue=ff6495ed",
            "Cornsilk=fffff8dc",
            "Crimson=ffdc143c",
            "Cyan=ff00ffff",
            "DarkBlue=ff00008b",
            "DarkCyan=ff008b8b",
            "DarkGoldenrod=ffb8860b",
            "DarkGray=ffa9a9a9",
            "DarkGreen=ff006400",
            "DarkKhaki=ffbdb76b",
            "DarkMagenta=ff8b008b",
            "DarkOliveGreen=ff556b2f",
            "DarkOrange=ffff8c00",
            "DarkOrchid=ff9932cc",
            "DarkRed=ff8b0000",
            "DarkSalmon=ffe9967a",
            "DarkSeaGreen=ff8fbc8f",
            "DarkSlateBlue=ff483d8b",
            "DarkSlateGray=ff2f4f4f",
            "DarkTurquoise=ff00ced1",
            "DarkViolet=ff9400d3",
            "DeepPink=ffff1493",
            "DeepSkyBlue=ff00bfff",
            "Default=ffffffff",
            "DimGray=ff696969",
            "DodgerBlue=ff1e90ff",
            "Firebrick=ffb22222",
            "FloralWhite=fffffaf0",
            "ForestGreen=ff228b22",
            "Fuchsia=ffff00ff",
            "Gainsboro=ffdcdcdc",
            "GhostWhite=fff8f8ff",
            "Gold=ffffd700",
            "Goldenrod=ffdaa520",
            "Gray=ff808080",
            "Green=ff008000",
            "GreenYellow=ffadff2f",
            "Honeydew=fff0fff0",
            "HotPink=ffff69b4",
            "IndianRed=ffcd5c5c",
            "Indigo=ff4b0082",
            "Ivory=fffffff0",
            "Khaki=fff0e68c",
            "Lavender=ffe6e6fa",
            "LavenderBlush=fffff0f5",
            "LawnGreen=ff7cfc00",
            "LemonChiffon=fffffacd",
            "LightBlue=ffadd8e6",
            "LightCoral=fff08080",
            "LightCyan=ffe0ffff",
            "LightGoldenrodYellow=fffafad2",
            "LightGray=ffd3d3d3",
            "LightGreen=ff90ee90",
            "LightPink=ffffb6c1",
            "LightSalmon=ffffa07a",
            "LightSeaGreen=ff20b2aa",
            "LightSkyBlue=ff87cefa",
            "LightSlateGray=ff778899",
            "LightSteelBlue=ffb0c4de",
            "LightYellow=ffffffe0",
            "Lime=ff00ff00",
            "LimeGreen=ff32cd32",
            "Linen=fffaf0e6",
            "Magenta=ffff00ff",
            "Maroon=ff800000",
            "MediumAquamarine=ff66cdaa",
            "MediumBlue=ff0000cd",
            "MediumOrchid=ffba55d3",
            "MediumPurple=ff9370db",
            "MediumSeaGreen=ff3cb371",
            "MediumSlateBlue=ff7b68ee",
            "MediumSpringGreen=ff00fa9a",
            "MediumTurquoise=ff48d1cc",
            "MediumVioletRed=ffc71585",
            "MidnightBlue=ff191970",
            "MintCream=fff5fffa",
            "MistyRose=ffffe4e1",
            "Moccasin=ffffe4b5",
            "NavajoWhite=ffffdead",
            "Navy=ff000080",
            "OldLace=fffdf5e6",
            "Olive=ff808000",
            "OliveDrab=ff6b8e23",
            "Orange=ffffa500",
            "OrangeRed=ffff4500",
            "Orchid=ffda70d6",
            "PaleGoldenrod=ffeee8aa",
            "PaleGreen=ff98fb98",
            "PaleTurquoise=ffafeeee",
            "PaleVioletRed=ffdb7093",
            "PapayaWhip=ffffefd5",
            "PeachPuff=ffffdab9",
            "Peru=ffcd853f",
            "Pink=ffffc0cb",
            "Plum=ffdda0dd",
            "PowderBlue=ffb0e0e6",
            "Purple=ff800080",
            "Red=ffff0000",
            "RosyBrown=ffbc8f8f",
            "RoyalBlue=ff4169e1",
            "SaddleBrown=ff8b4513",
            "Salmon=fffa8072",
            "SandyBrown=fff4a460",
            "SeaGreen=ff2e8b57",
            "SeaShell=fffff5ee",
            "Sienna=ffa0522d",
            "Silver=ffc0c0c0",
            "SkyBlue=ff87ceeb",
            "SlateBlue=ff6a5acd",
            "SlateGray=ff708090",
            "Snow=fffffafa",
            "SpringGreen=ff00ff7f",
            "SteelBlue=ff4682b4",
            "Tan=ffd2b48c",
            "Teal=ff008080",
            "Thistle=ffd8bfd8",
            "Tomato=ffff6347",
            "Transparent=00ffffff",
            "Turquoise=ff40e0d0",
            "Violet=ffee82ee",
            "Wheat=fff5deb3",
            "White=ffffffff",
            "WhiteSmoke=fff5f5f5",
            "Yellow=ffffff00",
            "YellowGreen=ff9acd32",
        };

    #endregion

    public ColorService(IConfiguration? configuration = null)
    {
        foreach (var spec in _colorNames)
        {
            try
            {
                var color = ColorModel.FromString(spec);
                if (!_colors.ContainsKey(color.Name))
                {
                    _colors.Add(color.Name, color);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing color spec '{spec}': {ex.Innermost()}");
            }
        }
        if (configuration is not null)
        {
            var customcolors = configuration.GetSection("CustomColors").Get<IEnumerable<string>>();
            if (customcolors is not null && customcolors.Any())
            {
                foreach (var spec in customcolors)
                {
                    try
                    {
                        var color = ColorModel.FromString(spec);
                        if (!_colors.ContainsKey(color.Name))
                        {
                            _colors.Add(color.Name, color);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing custom color spec '{spec}': {ex.Innermost()}");
                    }
                }
            }
        }
        if (!_colors.ContainsKey("default"))
        {
            _colors.Add("default", new("default", 255, 255, 255, 255));
        }
    }

    public IEnumerable<ColorModel> Colors => _colors
        .OrderBy(x => x.Key)
        .Select(x => x.Value)
        .ToList();

    public bool Exists(string name) => !string.IsNullOrWhiteSpace(name) && _colors.ContainsKey(name);

    public ColorModel? this[string key] => Exists(key) ? _colors[key] : null;

    public string GetHexString(string name)
    {
        if (Exists(name))
        {
            var color = _colors[name];
            return $"#{color.Value:x6}";
        }
        return "#000000";
    }

    public (byte A, byte R, byte G, byte B) GetARGB(string name)
    {
        if (Exists(name))
        {
            var color = _colors[name];
            return (color.A, color.R, color.G, color.B);
        }
        return (0, 0, 0, 0);
    }

    public ulong GetLongARGB(string name, bool aHigh = true)
    {
        if (Exists(name))
        {
            var color = _colors[name];
            return aHigh
                ? (ulong)(color.A << 24 | color.R << 16 | color.G << 8 | color.B)
                : (ulong)(color.B << 24 | color.G << 16 | color.R << 8 | color.A);
        }
        return 0;
    }

    public bool AddCustomColor(string name, byte a, byte r, byte g, byte b)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }
        if (Exists(name))
        {
            return false;
        }
        _colors.Add(name, new(name, a, r, g, b));
        return true;
    }

    public bool AddCustomColor(string name, ulong colorValue)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }
        if (Exists(name))
        {
            return false;
        }
        var a = (byte)(colorValue >> 24 & 0xff);
        var r = (byte)(colorValue >> 16 & 0xff);
        var g = (byte)(colorValue >> 8 & 0xff);
        var b = (byte)(colorValue & 0xff);
        _colors.Add(name, new(name, a, r, g, b));
        return true;
    }

    public bool IsBright(string name)
    {
        if (!_colors.ContainsKey(name))
        {
            return false;
        }
        var (_, r, g, b) = GetARGB(name);
        var brightness = ((r * 299) + (g * 587) + (b * 114)) / 1000; // 0.0 - 255.0
        return brightness > 128.0;
    }
}
