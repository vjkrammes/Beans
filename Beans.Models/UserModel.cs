using Beans.Common;
using Beans.Repositories.Entities;

using System.Globalization;

namespace Beans.Models;
public class UserModel : ModelBase, IEquatable<UserModel>, IComparable<UserModel>, IFormattable
{
    public string Id { get; set; }
    public string Identifier { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DisplayName { get; set; }
    public decimal Balance { get; set; }
    public decimal OwedToExchange { get; set; }
    public DateTime DateJoined { get; set; }
    public bool IsAdmin { get; set; }

    public UserModel() : base()
    {
        Id = IdEncoder.EncodeId(0);
        Identifier = string.Empty;
        Email = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        DisplayName = string.Empty;
        Balance = 0M;
        OwedToExchange = 0M;
        DateJoined = DateTime.UtcNow;
        IsAdmin = false;
    }

    public static UserModel? FromEntity(UserEntity entity) => entity is null ? null : new()
    {
        Id = IdEncoder.EncodeId(entity.Id),
        Identifier = entity.Identifier ?? string.Empty,
        Email = entity.Email ?? string.Empty,
        FirstName = entity.FirstName ?? string.Empty,
        LastName = entity.LastName ?? string.Empty,
        DisplayName = entity.DisplayName ?? string.Empty,
        Balance = entity.Balance,
        OwedToExchange = entity.OwedToExchange,
        DateJoined = entity.DateJoined,
        IsAdmin = entity.IsAdmin,
        CanDelete = true
    };

    public static UserEntity? FromModel(UserModel model) => model is null ? null : new()
    {
        Id = IdEncoder.DecodeId(model.Id),
        Identifier = model.Identifier ?? string.Empty,
        Email = model.Email ?? string.Empty,
        FirstName = model.FirstName ?? string.Empty,
        LastName = model.LastName ?? string.Empty,
        DisplayName = model.DisplayName ?? string.Empty,
        Balance = model.Balance,
        OwedToExchange = model.OwedToExchange,
        DateJoined = model.DateJoined,
        IsAdmin = model.IsAdmin,
    };

    public UserModel Clone() => new()
    {
        Id = Id,
        Identifier = Identifier ?? string.Empty,
        Email = Email ?? string.Empty,
        FirstName = FirstName ?? string.Empty,
        LastName = LastName ?? string.Empty,
        DisplayName = DisplayName ?? string.Empty,
        Balance = Balance,
        OwedToExchange = OwedToExchange,
        DateJoined = DateJoined,
        IsAdmin = IsAdmin,
        CanDelete = CanDelete
    };

    private string DefaultName() => ToString("fl");

    public override string ToString() => DefaultName();

    public string ToString(string format) => ToString(format, CultureInfo.CurrentCulture);

    public string ToString(string? format, IFormatProvider? provider)
    {
        if (string.IsNullOrWhiteSpace(format))
        {
            return DefaultName();
        }
        var culture = provider is null ? CultureInfo.CurrentCulture : (CultureInfo)provider;
        return NameModel.FromUserModel(this)!.ToString(format, culture);
    }

    public override bool Equals(object? obj) => obj is UserModel model && model.Id == Id;

    public bool Equals(UserModel? model) => model is not null && model.Id == Id;

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(UserModel left, UserModel right) => (left, right) switch
    {
        (null, null) => true,
        (null, _) or (_, null) => false,
        (_, _) => left.Id == right.Id
    };

    public static bool operator !=(UserModel left, UserModel right) => !(left == right);

    public int CompareTo(UserModel? other) => Email.CompareTo(other?.Email);

    public static bool operator >(UserModel left, UserModel right) => left.CompareTo(right) > 0;

    public static bool operator <(UserModel left, UserModel right) => left.CompareTo(right) < 0;

    public static bool operator >=(UserModel left, UserModel right) => left.CompareTo(right) >= 0;

    public static bool operator <=(UserModel left, UserModel right) => left.CompareTo(right) <= 0;

    public static implicit operator UserModel?(UserEntity entity) => FromEntity(entity);

    public static implicit operator UserEntity?(UserModel model) => FromModel(model);
}
