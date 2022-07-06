using Beans.Repositories.Entities;

namespace Beans.Models;

public class SettingsModel : ModelBase, IEquatable<SettingsModel>, IComparable<SettingsModel>
{
    public string Name { get; set; }
    public string Value { get; set; }

    public SettingsModel()
    {
        Name = string.Empty;
        Value = string.Empty;
    }

    public static SettingsModel? FromEntity(SettingsEntity entity) => entity is null ? null : new()
    {
        Name = entity.Name ?? string.Empty,
        Value = entity.Value ?? string.Empty,
        CanDelete = true
    };

    public static SettingsEntity? FromModel(SettingsModel model) => model is null ? null : new()
    {
        Name = model.Name ?? string.Empty,
        Value = model.Value ?? string.Empty
    };

    public SettingsModel Clone() => new()
    {
        Name = Name ?? string.Empty,
        Value = Value ?? string.Empty,
        CanDelete = CanDelete
    };

    public override string ToString() => Name;

    public override bool Equals(object? obj) => obj is SettingsModel model && model.Name == Name;

    public bool Equals(SettingsModel? model) => model is not null && model.Name == Name;

    public override int GetHashCode() => Name.GetHashCode();

    public static bool operator ==(SettingsModel left, SettingsModel right) => (left, right) switch
    {
        (null, null) => true,
        (null, _) or (_, null) => false,
        (_, _) => left.Name == right.Name
    };

    public static bool operator !=(SettingsModel left, SettingsModel right) => !(left == right);

    public int CompareTo(SettingsModel? other) => Name.CompareTo(other?.Name);

    public static bool operator >(SettingsModel left, SettingsModel right) => left.CompareTo(right) > 0;

    public static bool operator <(SettingsModel left, SettingsModel right) => left.CompareTo(right) < 0;

    public static bool operator >=(SettingsModel left, SettingsModel right) => left.CompareTo(right) >= 0;

    public static bool operator <=(SettingsModel left, SettingsModel right) => left.CompareTo(right) <= 0;

    public static implicit operator SettingsModel?(SettingsEntity entity) => FromEntity(entity);

    public static implicit operator SettingsEntity?(SettingsModel model) => FromModel(model);
}
