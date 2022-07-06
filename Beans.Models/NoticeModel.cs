using Beans.Common;
using Beans.Repositories.Entities;

namespace Beans.Models;
public class NoticeModel : ModelBase, IEquatable<NoticeModel>, IComparable<NoticeModel>
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string SenderId { get; set; }
    public DateTime NoticeDate { get; set; }
    public string Title { get; set; }
    public string Text { get; set; }
    public bool Read { get; set; }
    public bool SenderIsSystem { get; init; }
    public bool SenderIsExchange { get; init; }

    public UserModel? Sender { get; set; }

    public NoticeModel() : base()
    {
        Id = IdEncoder.EncodeId(0);
        UserId = IdEncoder.EncodeId(0);
        SenderId = IdEncoder.EncodeId(0);
        NoticeDate = DateTime.UtcNow;
        Title = string.Empty;
        Text = string.Empty;
        Read = false;
        SenderIsSystem = false;
        SenderIsExchange = false;
        Sender = null;
    }

    public static NoticeModel? FromEntity(NoticeEntity entity) => entity is null ? null : new()
    {
        Id = IdEncoder.EncodeId(entity.Id),
        UserId = IdEncoder.EncodeId(entity.UserId),
        SenderId = entity.Id >= 0 ? IdEncoder.EncodeId(entity.SenderId) : string.Empty,
        NoticeDate = entity.NoticeDate,
        Title = entity.Title ?? string.Empty,
        Text = entity.Text ?? string.Empty,
        Read = entity.Read,
        SenderIsSystem = entity.SenderId == Constants.SENDER_IS_SYSTEM,
        SenderIsExchange = entity.SenderId == Constants.SENDER_IS_EXCHANGE,
        Sender = entity.Sender!,
        CanDelete = true
    };

    public static NoticeEntity? FromModel(NoticeModel model) => model is null ? null : new()
    {
        Id = IdEncoder.DecodeId(model.Id),
        UserId = IdEncoder.DecodeId(model.UserId),
        SenderId = model.SenderIsExchange ? Constants.SENDER_IS_EXCHANGE : model.SenderIsSystem ? Constants.SENDER_IS_SYSTEM : IdEncoder.DecodeId(model.SenderId),
        NoticeDate = model.NoticeDate,
        Title = model.Title ?? string.Empty,
        Text = model.Text ?? string.Empty,
        Sender = model.Sender!,
        Read = model.Read
    };

    public NoticeModel Clone() => new()
    {
        Id = Id,
        UserId = UserId,
        SenderId = SenderId,
        NoticeDate = NoticeDate,
        Title = Title ?? string.Empty,
        Text = Text ?? string.Empty,
        Read = Read,
        SenderIsSystem = SenderIsSystem,
        SenderIsExchange = SenderIsExchange,
        Sender = Sender?.Clone(),
        CanDelete = CanDelete
    };

    public override string ToString() => $"({NoticeDate.ToShortDateString()}) {Title.Beginning(25)}";

    public override bool Equals(object? obj) => obj is NoticeModel model && model.Id == Id;

    public bool Equals(NoticeModel? model) => model is not null && model.Id == Id;

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(NoticeModel left, NoticeModel right) => (left, right) switch
    {
        (null, null) => true,
        (null, _) or (_, null) => false,
        (_, _) => left.Id == right.Id
    };

    public static bool operator !=(NoticeModel left, NoticeModel right) => !(left == right);

    public int CompareTo(NoticeModel? other) => NoticeDate.CompareTo(other?.NoticeDate);

    public static bool operator >(NoticeModel left, NoticeModel right) => left.CompareTo(right) > 0;

    public static bool operator <(NoticeModel left, NoticeModel right) => left.CompareTo(right) < 0;

    public static bool operator >=(NoticeModel left, NoticeModel right) => left.CompareTo(right) >= 0;

    public static bool operator <=(NoticeModel left, NoticeModel right) => left.CompareTo(right) <= 0;

    public static implicit operator NoticeModel?(NoticeEntity entity) => FromEntity(entity);

    public static implicit operator NoticeEntity?(NoticeModel model) => FromModel(model);
}
