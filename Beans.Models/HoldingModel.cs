using Beans.Common;
using Beans.Repositories.Entities;

namespace Beans.Models;
public class HoldingModel : ModelBase, IEquatable<HoldingModel>, IComparable<HoldingModel>
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string BeanId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public long Quantity { get; set; }
    public decimal Price { get; set; }

    public BeanModel? Bean { get; set; }

    public HoldingModel()
    {
        Id = IdEncoder.EncodeId(0);
        UserId = IdEncoder.EncodeId(0);
        BeanId = IdEncoder.EncodeId(0);
        PurchaseDate = default;
        Quantity = 0;
        Price = 0M;
        Bean = null;
    }

    public static HoldingModel? FromEntity(HoldingEntity entity) => entity is null ? null : new()
    {
        Id = IdEncoder.EncodeId(entity.Id),
        UserId = IdEncoder.EncodeId(entity.UserId),
        BeanId = IdEncoder.EncodeId(entity.BeanId),
        PurchaseDate = entity.PurchaseDate,
        Quantity = entity.Quantity,
        Price = entity.Price,
        Bean = entity.Bean!,
        CanDelete = true
    };

    public static HoldingEntity? FromModel(HoldingModel model) => model is null ? null : new()
    {
        Id = IdEncoder.DecodeId(model.Id),
        UserId = IdEncoder.DecodeId(model.UserId),
        BeanId = IdEncoder.DecodeId(model.BeanId),
        PurchaseDate = model.PurchaseDate,
        Quantity = model.Quantity,
        Price = model.Price,
        Bean = model.Bean!
    };

    public HoldingModel Clone() => new()
    {
        Id = Id,
        UserId = UserId,
        BeanId = BeanId,
        PurchaseDate = PurchaseDate,
        Quantity = Quantity,
        Price = Price,
        Bean = Bean?.Clone(),
        CanDelete = CanDelete
    };

    public override string ToString() => $"{PurchaseDate.ToShortDateString()} ({Quantity} x {Price.ToCurrency(2)})";

    public override bool Equals(object? obj) => obj is HoldingModel model && model.Id == Id;

    public bool Equals(HoldingModel? model) => model is not null && model.Id == Id;

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(HoldingModel left, HoldingModel right) => (left, right) switch
    {
        (null, null) => true,
        (null, _) or (_, null) => false,
        (_, _) => left.Id == right.Id
    };

    public static bool operator !=(HoldingModel left, HoldingModel right) => !(left == right);

    public int CompareTo(HoldingModel? other) => PurchaseDate.CompareTo(other?.PurchaseDate);

    public static bool operator >(HoldingModel left, HoldingModel right) => left.CompareTo(right) > 0;

    public static bool operator <(HoldingModel left, HoldingModel right) => left.CompareTo(right) < 0;

    public static bool operator >=(HoldingModel left, HoldingModel right) => left.CompareTo(right) >= 0;

    public static bool operator <=(HoldingModel left, HoldingModel right) => left.CompareTo(right) <= 0;

    public static implicit operator HoldingModel?(HoldingEntity entity) => FromEntity(entity);

    public static implicit operator HoldingEntity?(HoldingModel model) => FromModel(model);
}
