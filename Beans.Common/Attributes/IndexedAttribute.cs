namespace Beans.Common.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class IndexedAttribute : Attribute
{
    public string IndexName { get; }
    public IndexedAttribute() => IndexName = string.Empty;
    public IndexedAttribute(string indexName) => IndexName = indexName;
}
