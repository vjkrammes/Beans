using System.Reflection;
using System.Text;

namespace Beans.Common;
public static class Tools
{
    public static bool Any(params bool[] items) => items is not null && items.Any();

    public static bool All(params bool[] items) => items is not null && items.All(x => x);

    public static T? LeastOf<T>(params T[] items) where T : IComparable<T> => items.Min();

    public static T? GreatestOf<T>(params T[] items) where T : IComparable<T> => items.Max();

    public static string DumpObject<T>(T item)
    {
        if (item is null)
        {
            throw new ArgumentNullException(nameof(item));
        }
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var sb = new StringBuilder($"Object Type: {typeof(T).FullName}:");
        sb.AppendLine();
        foreach (var property in properties)
        {
            var value = property.GetValue(item);
            if (value is not null)
            {
                sb.AppendLine($"'{property.Name}' = '{value}'");
            }
            else
            {
                sb.AppendLine($"'{property.Name}' = <null>");
            }
        }
        return sb.ToString();
    }
}
