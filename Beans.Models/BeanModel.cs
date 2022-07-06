using Beans.Common;
using Beans.Repositories.Entities;

namespace Beans.Models;
public class BeanModel : ModelBase, IEquatable<BeanModel>, IComparable<BeanModel>
{
    public string Id { get; set; }
    public string Name { get; set; }
    public long ARGB { get; set; }
    public decimal Price { get; set; }
    public long Quantity { get; set; }
    public long Held { get; set; }
    public long ExchangeHeld { get; set; }
    public string Filename { get; set; }

    public BeanModel() : base()
    {
        Id = IdEncoder.EncodeId(0);
        Name = string.Empty;
        ARGB = 0;
        Price = 0M;
        Quantity = 0;
        Held = 0;
        ExchangeHeld = 0;
        Filename = string.Empty;
    }

    public static BeanModel? FromEntity(BeanEntity? entity) => entity is null ? null : new()
    {
        Id = IdEncoder.EncodeId(entity.Id),
        Name = entity.Name ?? string.Empty,
        ARGB = entity.ARGB,
        Price = entity.Price,
        Quantity = entity.Quantity,
        Held = entity.Held,
        ExchangeHeld = entity.ExchangeHeld,
        Filename = entity.Filename ?? string.Empty,
        CanDelete = true
    };

    public static BeanEntity? FromModel(BeanModel? model) => model is null ? null : new()
    {
        Id = IdEncoder.DecodeId(model.Id),
        Name = model.Name ?? string.Empty,
        ARGB = model.ARGB,
        Price = model.Price,
        Quantity = model.Quantity,
        Held = model.Held,
        ExchangeHeld = model.ExchangeHeld,
        Filename = model.Filename ?? string.Empty
    };

    public BeanModel Clone() => new()
    {
        Id = Id,
        Name = Name ?? string.Empty,
        ARGB = ARGB,
        Price = Price,
        Quantity = Quantity,
        Held = Held,
        ExchangeHeld = ExchangeHeld,
        Filename = Filename ?? string.Empty,
        CanDelete = CanDelete
    };

    public override string ToString() => Name;

    public override bool Equals(object? obj) => obj is BeanModel model && model.Id == Id;

    public bool Equals(BeanModel? model) => model is not null && model.Id == Id;

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(BeanModel left, BeanModel right) => (left, right) switch
    {
        (null, null) => true,
        (null, _) or (_, null) => false,
        (_, _) => left.Id == right.Id
    };

    public static bool operator !=(BeanModel left, BeanModel right) => !(left == right);

    public int CompareTo(BeanModel? other) => Name.CompareTo(other?.Name);

    public static bool operator >(BeanModel left, BeanModel right) => left.CompareTo(right) > 0;

    public static bool operator <(BeanModel left, BeanModel right) => left.CompareTo(right) < 0;

    public static bool operator >=(BeanModel left, BeanModel right) => left.CompareTo(right) >= 0;

    public static bool operator <=(BeanModel left, BeanModel right) => left.CompareTo(right) <= 0;

    public static implicit operator BeanModel?(BeanEntity entity) => FromEntity(entity);

    public static implicit operator BeanEntity?(BeanModel model) => FromModel(model);
}
