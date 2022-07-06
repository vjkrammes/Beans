using Beans.Common;

using System.Globalization;
using System.Text;

namespace Beans.Models;
public class NameModel : IEquatable<NameModel>, IFormattable
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DisplayName { get; set; }
    public string FirstLast { get; set; }
    public string LastFirst { get; set; }
    public bool IsAdmin { get; set; }

    public NameModel()
    {
        Id = IdEncoder.EncodeId(0);
        Email = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        DisplayName = string.Empty;
        FirstLast = string.Empty;
        LastFirst = string.Empty;
        IsAdmin = false;
    }

    public static NameModel? FromUserModel(UserModel model) => model is null ? null : new()
    {
        Id = model.Id,
        Email = model.Email,
        FirstName = model.FirstName,
        LastName = model.LastName,
        DisplayName = model.DisplayName,
        FirstLast = string.IsNullOrWhiteSpace(model.FirstName) ? model.LastName : $"{model.FirstName} {model.LastName}",
        LastFirst = string.IsNullOrWhiteSpace(model.FirstName) ? model.LastName : $"{model.LastName}, {model.FirstName}",
        IsAdmin = model.IsAdmin,
    };

    public static IEnumerable<NameModel> FromUserModels(IEnumerable<UserModel> models)
    {
        var ret = new List<NameModel>();
        if (models is not null && models.Any())
        {
            models.ForEach(x => ret.Add(FromUserModel(x)!));
        }
        return ret;
    }

    public string DefaultName => DisplayName;

    public override string ToString() => ToString("lf", CultureInfo.CurrentCulture);

    public string ToString(string format) => ToString(format, CultureInfo.CurrentCulture);

    public string ToString(string? format, IFormatProvider? provider)
    {
        if (string.IsNullOrWhiteSpace(format))
        {
            return DefaultName;
        }
        var culture = provider is null ? CultureInfo.CurrentCulture : (CultureInfo)provider;
        return format.ToLower(culture) switch
        {
            "lf" or "l" => LastFirst,
            "fl" or "f" => FirstLast,
            "ex" or "x" => ExtendedName(),
            "d" => DisplayName,
            _ => DefaultName
        };
    }

    public string ExtendedName()
    {
        if (Id == "0") // special case for html <select>
        {
            return LastFirst;
        }
        StringBuilder sb = new(LastName);
        if (!string.IsNullOrWhiteSpace(FirstName))
        {
            sb.Append(", ");
            sb.Append(FirstName);
        }
        if (IsAdmin)
        {
            sb.Append(" (admin)");
        }
        return sb.ToString();
    }

    public override bool Equals(object? obj) => obj is NameModel model && model.Id == Id;

    public bool Equals(NameModel? model) => model is not null && model.Id == Id;

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(NameModel left, NameModel right) => (left, right) switch
    {
        (null, null) => true,
        (null, _) or (_, null) => false,
        (_, _) => left.Id == right.Id
    };

    public static bool operator !=(NameModel left, NameModel right) => !(left == right);
}
