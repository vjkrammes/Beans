namespace Beans.Common.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ColumnNameAttribute : Attribute
{
    public string ColumnName { get; }
    public ColumnNameAttribute(string name) => ColumnName = name;
}
