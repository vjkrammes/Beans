using Beans.Common;
using Beans.Repositories.Entities;

namespace Beans.Models;
public class SaleModel : ModelBase, IEquatable<SaleModel>, IComparable<SaleModel>
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string BeanId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime SaleDate { get; set; }
    public bool LongTerm { get; set; }
    public long Quantity { get; set; }
    public decimal CostBasis { get; set; }
    public decimal SalePrice { get; set; }
    public BeanModel? Bean { get; set; }
    public decimal GainOrLoss => (Quantity * SalePrice) - (Quantity * CostBasis);

    public SaleModel() : base()
    {
        Id = IdEncoder.EncodeId(0);
        UserId = IdEncoder.EncodeId(0);
        BeanId = IdEncoder.EncodeId(0);
        PurchaseDate = default;
        SaleDate = DateTime.UtcNow;
        LongTerm = false;
        Quantity = 0;
        CostBasis = 0M;
        SalePrice = 0M;
        Bean = null;
    }

    public static SaleModel? FromEntity(SaleEntity entity) => entity is null ? null : new()
    {
        Id = IdEncoder.EncodeId(entity.Id),
        UserId = IdEncoder.EncodeId(entity.UserId),
        BeanId = IdEncoder.EncodeId(entity.BeanId),
        PurchaseDate = entity.PurchaseDate,
        SaleDate = entity.SaleDate,
        LongTerm = false,
        Quantity = entity.Quantity,
        CostBasis = entity.CostBasis,
        SalePrice = entity.SalePrice,
        Bean = entity.Bean!,
        CanDelete = true
    };

    public static SaleEntity? FromModel(SaleModel model) => model is null ? null : new()
    {
        Id = IdEncoder.DecodeId(model.Id),
        UserId = IdEncoder.DecodeId(model.UserId),
        BeanId = IdEncoder.DecodeId(model.BeanId),
        PurchaseDate = model.PurchaseDate,
        SaleDate = model.SaleDate,
        Quantity = model.Quantity,
        CostBasis = model.CostBasis,
        SalePrice = model.SalePrice,
        Bean = model.Bean!,
    };

    public SaleModel Clone() => new()
    {
        Id = Id,
        UserId = UserId,
        BeanId = BeanId,
        PurchaseDate = PurchaseDate,
        SaleDate = SaleDate,
        LongTerm = LongTerm,
        Quantity = Quantity,
        CostBasis = CostBasis,
        SalePrice = SalePrice,
        Bean = Bean?.Clone(),
        CanDelete = CanDelete
    };

    public override string ToString() => $"{Quantity} @ {SalePrice.ToCurrency(2)} on {SaleDate.ToShortDateString()}";

    public override bool Equals(object? obj) => obj is SaleModel model && model.Id == Id;

    public bool Equals(SaleModel? model) => model is not null && model.Id == Id;

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(SaleModel left, SaleModel right) => (left, right) switch
    {
        (null, null) => true,
        (null, _) or (_, null) => false,
        (_, _) => left.Id == right.Id
    };

    public static bool operator !=(SaleModel left, SaleModel right) => !(left == right);

    public int CompareTo(SaleModel? other) => SaleDate.CompareTo(other?.SaleDate);

    public static bool operator >(SaleModel left, SaleModel right) => left.CompareTo(right) > 0;

    public static bool operator <(SaleModel left, SaleModel right) => left.CompareTo(right) < 0;

    public static bool operator >=(SaleModel left, SaleModel right) => left.CompareTo(right) >= 0;

    public static bool operator <=(SaleModel left, SaleModel right) => left.CompareTo(right) <= 0;

    public static implicit operator SaleModel?(SaleEntity entity) => FromEntity(entity);

    public static implicit operator SaleEntity?(SaleModel model) => FromModel(model);
}
