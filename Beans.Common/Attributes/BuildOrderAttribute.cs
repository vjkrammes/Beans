namespace Beans.Common.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class BuildOrderAttribute : Attribute
{
    public int BuildOrder { get; }
    public BuildOrderAttribute(int order) => BuildOrder = order;
}
