using Beans.Common.Attributes;

using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Beans.Common;

public static class ExtensionMethods
{
    public static string ToCurrency(this decimal value, int decimals = 2) =>
      value.ToString($"c{decimals}").Replace(CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol, Constants.CurrencySymbol);

    public static string ToSignedCurrency(this decimal value, int decimals = 2)
    {
        var ret = value.ToCurrency(decimals);
        ret = ret.Replace("(", "").Replace(")", "");
        return value < 0 ? "-" + ret : ret;
    }

    public static string ToUnsignedCurrency(this decimal value, int decimals = 2)
    {
        var ret = value.ToCurrency(decimals);
        ret = ret.Replace("(", "").Replace(")", "");
        return ret;
    }

    public static string FromCurrency(this string value) => value.Replace(Constants.CurrencySymbol, "");

    public static double StandardDeviation(this IEnumerable<double> values)
    {
        if (values is null || !values.Any() || values.Count() == 1)
        {
            return 0.0;
        }
        var average = values.Average();
        var sum = values.Sum(x => Math.Pow(x - average, 2));
        return Math.Sqrt(sum / (values.Count() - 1));
    }

    public static decimal StandardDeviation(this IEnumerable<decimal> values) => (decimal)values.Select(x => (double)x).StandardDeviation();

    public static string FirstWord(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }
        var ret = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).First();
        return ret;
    }

    public static string Host(this string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return string.Empty;
        }
        try
        {
            var uri = new Uri(url);
            return uri.Host;
        }
        catch
        {
            return string.Empty;
        }
    }

    public static int Sign<T>(this T value) where T : IComparable<T>
    {
        if (value.CompareTo(default) < 0)
        {
            return -1;
        }
        if (value.CompareTo(default) > 0)
        {
            return 1;
        }
        return 0;
    }

    public static string StripPadding(this string value) => value.TrimEnd('=');

    public static string AddPadding(this string value) => (value.Length % 4) switch
    {
        0 => value,
        1 => value + "===",
        2 => value + "==",
        _ => value + "="
    };

    public static (byte o0, byte o1, byte o2, byte o3) Octets(this ulong value)
    {
        var o0 = (byte)((value >> 24) & 0xff);
        var o1 = (byte)((value >> 16) & 0xff);
        var o2 = (byte)((value >> 8) & 0xff);
        var o3 = (byte)(value & 0xff);
        return (o0, o1, o2, o3);
    }

    public static (byte o0, byte o1, byte o2, byte o3) Octets(this long value) => ((ulong)value).Octets();

    public static (byte o0, byte o1, byte o2, byte o3) Octets(this uint value) => ((ulong)value).Octets();

    public static (byte o0, byte o1, byte o2, byte o3) Octets(this int value) => ((ulong)value).Octets();

    public static string Hexify(this ulong argb)
    {
        StringBuilder sb = new("0x");
        var (o0, o1, o2, o3) = argb.Octets();
        sb.Append(o0.ToString("x2"));
        sb.Append(o1.ToString("x2"));
        sb.Append(o2.ToString("x2"));
        sb.Append(o3.ToString("x2"));
        return sb.ToString();
    }

    public static string Hexify(this long argb) => ((ulong)argb).Hexify();

    public static string Hexify(this uint argb) => ((ulong)argb).Hexify();

    public static string Hexify(this int argb) => ((ulong)argb).Hexify();

    public static string Hexify(this byte[] value, bool addHeader = true)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }
        if (!value.Any())
        {
            return string.Empty;
        }
        var sb = new StringBuilder();
        if (addHeader)
        {
            sb.Append("0x");
        }
        foreach (var b in value)
        {
            sb.Append(b.ToString("x2"));
        }
        return sb.ToString();
    }

    public static IEnumerable<TModel> ToModels<TModel, TEntity>(this IEnumerable<TEntity> entities, string methodName = "FromEntity")
      where TModel : class where TEntity : class
    {
        var ret = new List<TModel>();
        var method = typeof(TModel).GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, null,
          new[] { typeof(TEntity) }, Array.Empty<ParameterModifier>());
        if (method is not null)
        {
            if (entities is not null && entities.Any())
            {
                entities.ForEach(x => ret.Add((method.Invoke(null, new[] { x }) as TModel)!));
            }
        }
        return ret;
    }

    public static string TrimEnd(this string value, string suffix, StringComparison comparer = StringComparison.OrdinalIgnoreCase)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }
        if (!string.IsNullOrWhiteSpace(suffix) && value.EndsWith(suffix, comparer))
        {
            return value[..^suffix.Length];
        }
        return value;
    }

    public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
    {
        if (list is null)
        {
            throw new ArgumentNullException(nameof(list));
        }
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }
        foreach (var item in list)
        {
            action(item);
        }
    }

    public static string Beginning(this string value, int length, char ellipsis = '.')
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }
        return value.Length <= length ? value : value[..length] + new string(ellipsis, 3);
    }

    public static bool IsDescending(this char direction) => direction is 'd' or 'D';

    public static T[] ArrayCopy<T>(this T[] source)
    {
        if (source is null)
        {
            return Array.Empty<T>();
        }
        var ret = new T[source.Length];
        Array.Copy(source, ret, source.Length);
        return ret;
    }

    public static bool ArrayEquals<T>(this T[] left, T[] right, bool wholeLength = false) where T : IEquatable<T>
    {
        if (left is null)
        {
            if (right is null)
            {
                return true;
            }
            return false;
        }
        if (right is null)
        {
            return false;
        }
        if (ReferenceEquals(left, right))
        {
            return true;
        }
        if (left.Length != right.Length)
        {
            return false;
        }
        var comparer = EqualityComparer<T>.Default;
        var ret = true;
        for (var i = 0; i < left.Length; i++)
        {
            if (!comparer.Equals(right[i], left[i]))
            {
                ret = false;
                if (!wholeLength)
                {
                    break;
                }
            }
        }
        return ret;
    }

    public static T[] Append<T>(this T[] left, T[] right)
    {
        if (left is null || !left.Any())
        {
            return right ?? Array.Empty<T>();
        }
        if (right is null || !right.Any())
        {
            return left;
        }
        var ret = new T[left.Length + right.Length];
        Array.Copy(left, 0, ret, 0, left.Length);
        Array.Copy(right, 0, ret, left.Length, right.Length);
        return ret;
    }

    public static string Capitalize(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }
        return value.First().ToString().ToUpper(CultureInfo.CurrentCulture) + string.Join(string.Empty, value.Skip(1));
    }

    public static string GetDescriptionFromEnumValue<T>(this T value) where T : Enum =>
      typeof(T)
        .GetField(value.ToString())!
        .GetCustomAttributes(typeof(DescriptionAttribute), false)
        .SingleOrDefault() is not DescriptionAttribute attribute ? value.ToString() : attribute.Description;

    public static string GetForegroundFromEnumValue<T>(this T value) where T : Enum =>
      typeof(T)
        .GetField(value.ToString())!
        .GetCustomAttributes(typeof(TextColorAttribute), false)
        .SingleOrDefault() is not TextColorAttribute attribute ? "Black" : attribute.Foreground;


    public static string GetBackgroundFromEnumValue<T>(this T value) where T : Enum =>
      typeof(T)
        .GetField(value.ToString())!
        .GetCustomAttributes(typeof(TextColorAttribute))
        .SingleOrDefault() is not TextColorAttribute attribute ? "Transparent" : attribute.Background;

    public static string Innermost(this Exception exception)
    {
        if (exception is null)
        {
            throw new ArgumentNullException(nameof(exception));
        }
        if (exception.InnerException is null)
        {
            return exception.Message;
        }
        return exception.InnerException.Innermost();
    }

    public static bool IsBetween(DateTime date, DateTime start, DateTime end) => date >= start && date <= end;
}
