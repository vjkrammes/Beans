namespace Beans.Common.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class ColorAttribute : Attribute
{
    public string Color { get; }
    public ColorAttribute(string color) => Color = color;
}
