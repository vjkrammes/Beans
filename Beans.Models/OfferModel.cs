using Beans.Common;
using Beans.Repositories.Entities;

namespace Beans.Models;
public class OfferModel : ModelBase, IEquatable<OfferModel>, IComparable<OfferModel>
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string BeanId { get; set; }
    public string HoldingId { get; set; }
    public long Quantity { get; set; }
    public decimal Price { get; set; }
    public bool Buy { get; set; }
    public DateTime OfferDate { get; set; }
    public BeanModel? Bean { get; set; }
    public UserModel? User { get; set; }
    public HoldingModel? Holding { get; set; }

    public OfferModel() : base()
    {
        Id = IdEncoder.EncodeId(0);
        UserId = IdEncoder.EncodeId(0);
        BeanId = IdEncoder.EncodeId(0);
        HoldingId = IdEncoder.EncodeId(0);
        Quantity = 0;
        Price = 0M;
        Buy = false;
        OfferDate = default;
        Bean = null;
        User = null;
        Holding = null;
    }

    public static OfferModel? FromEntity(OfferEntity entity) => entity is null ? null : new()
    {
        Id = IdEncoder.EncodeId(entity.Id),
        UserId = IdEncoder.EncodeId(entity.UserId),
        BeanId = IdEncoder.EncodeId(entity.BeanId),
        HoldingId = IdEncoder.EncodeId(entity.HoldingId),
        Quantity = entity.Quantity,
        Price = entity.Price,
        Buy = entity.Buy,
        OfferDate = entity.OfferDate,
        Bean = entity.Bean!,
        User = entity.User!,
        Holding = entity.Holding!,
        CanDelete = true
    };

    public static OfferEntity? FromModel(OfferModel model) => model is null ? null : new()
    {
        Id = IdEncoder.DecodeId(model.Id),
        UserId = IdEncoder.DecodeId(model.UserId),
        BeanId = IdEncoder.DecodeId(model.BeanId),
        HoldingId = IdEncoder.DecodeId(model.HoldingId),
        Quantity = model.Quantity,
        Price = model.Price,
        Buy = model.Buy,
        OfferDate = model.OfferDate,
        Bean = model.Bean!,
        User = model.User!,
        Holding = model.Holding!
    };

    public OfferModel Clone() => new()
    {
        Id = Id,
        UserId = UserId,
        BeanId = BeanId,
        HoldingId = HoldingId,
        Quantity = Quantity,
        Price = Price,
        Buy = Buy,
        OfferDate = OfferDate,
        Bean = Bean?.Clone(),
        User = User?.Clone(),
        Holding = Holding?.Clone(),
        CanDelete = CanDelete
    };

    public override string ToString() => $"{(Buy ? "Buy" : "Sell")} {Quantity} {Bean?.Name ?? "Unknown"} @ {Price.ToCurrency(2)}";

    public override bool Equals(object? obj) => obj is OfferModel model && model.Id == Id;

    public bool Equals(OfferModel? model) => model is not null && model.Id == Id;

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(OfferModel left, OfferModel right) => (left, right) switch
    {
        (null, null) => true,
        (null, _) or (_, null) => false,
        (_, _) => left.Id == right.Id
    };

    public static bool operator !=(OfferModel left, OfferModel right) => !(left == right);

    public int CompareTo(OfferModel? other) => OfferDate.CompareTo(other?.OfferDate);

    public static bool operator >(OfferModel left, OfferModel right) => left.CompareTo(right) > 0;

    public static bool operator <(OfferModel left, OfferModel right) => left.CompareTo(right) < 0;

    public static bool operator >=(OfferModel left, OfferModel right) => left.CompareTo(right) >= 0;

    public static bool operator <=(OfferModel left, OfferModel right) => left.CompareTo(right) <= 0;

    public static implicit operator OfferModel?(OfferEntity entity) => FromEntity(entity);

    public static implicit operator OfferEntity?(OfferModel model) => FromModel(model);
}
