namespace Beans.Common.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class TextColorAttribute : Attribute
{
    public string Foreground { get; }
    public string Background { get; }

    public TextColorAttribute(string foreground, string? background = null)
    {
        Foreground = foreground;
        Background = string.IsNullOrWhiteSpace(background) ? "Transparent" : background;
    }
}
