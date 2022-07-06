using Beans.Common;
using Beans.Common.Enumerations;
using Beans.Repositories.Entities;

using Dapper.Contrib.Extensions;

namespace Beans.Models;
public class MovementModel : ModelBase, IEquatable<MovementModel>, IComparable<MovementModel>
{
    public string Id { get; set; }
    public string BeanId { get; set; }
    public DateTime MovementDate { get; set; }
    public decimal Open { get; set; }
    public decimal Close { get; set; }
    public decimal Movement { get; set; }
    public MovementType MovementType { get; set; }

    public BeanModel? Bean { get; set; }

    [Write(false)]
    public decimal Percent => Open == 0M ? 0M : Movement / Open;

    public MovementModel()
    {
        Id = IdEncoder.EncodeId(0);
        BeanId = IdEncoder.EncodeId(0);
        MovementDate = default;
        Open = 0M;
        Close = 0M;
        Movement = 0M;
        MovementType = MovementType.Unspecified;
        Bean = null;
    }

    public static MovementModel? FromEntity(MovementEntity entity) => entity is null ? null : new()
    {
        Id = IdEncoder.EncodeId(entity.Id),
        BeanId = IdEncoder.EncodeId(entity.BeanId),
        MovementDate = entity.MovementDate,
        Open = entity.Open,
        Close = entity.Close,
        Movement = entity.Movement,
        MovementType = entity.MovementType,
        Bean = entity.Bean!,
        CanDelete = true
    };

    public static MovementEntity? FromModel(MovementModel model) => model is null ? null : new()
    {
        Id = IdEncoder.DecodeId(model.Id),
        BeanId = IdEncoder.DecodeId(model.BeanId),
        MovementDate = model.MovementDate,
        Open = model.Open,
        Close = model.Close,
        Movement = model.Movement,
        MovementType = model.MovementType,
        Bean = model.Bean!,
    };

    public MovementModel Clone() => new()
    {
        Id = Id,
        BeanId = BeanId,
        MovementDate = MovementDate,
        Open = Open,
        Close = Close,
        Movement = Movement,
        MovementType = MovementType,
        Bean = Bean?.Clone(),
        CanDelete = CanDelete
    };

    public override string ToString() =>
      $"{MovementDate.ToShortDateString()}: Open: {Open.ToCurrency(2)}, Close: {Close.ToCurrency(2)}: {Movement.ToCurrency(2)}";

    public override bool Equals(object? obj) => obj is MovementModel model && model.Id == Id;

    public bool Equals(MovementModel? model) => model is not null && model.Id == Id;

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(MovementModel left, MovementModel right) => (left, right) switch
    {
        (null, null) => true,
        (null, _) or (_, null) => false,
        (_, _) => left.Id == right.Id
    };

    public static bool operator !=(MovementModel left, MovementModel right) => !(left == right);

    public int CompareTo(MovementModel? other) => MovementDate.CompareTo(other?.MovementDate);

    public static bool operator >(MovementModel left, MovementModel right) => left.CompareTo(right) > 0;

    public static bool operator <(MovementModel left, MovementModel right) => left.CompareTo(right) < 0;

    public static bool operator >=(MovementModel left, MovementModel right) => left.CompareTo(right) >= 0;

    public static bool operator <=(MovementModel left, MovementModel right) => left.CompareTo(right) <= 0;

    public static implicit operator MovementModel?(MovementEntity entity) => FromEntity(entity);

    public static implicit operator MovementEntity?(MovementModel model) => FromModel(model);
}
