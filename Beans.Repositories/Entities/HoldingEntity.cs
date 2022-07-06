using Beans.Common;
using Beans.Common.Attributes;
using Beans.Common.Interfaces;

using Dapper.Contrib.Extensions;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Beans.Repositories.Entities;

[Table("Holdings")]
[BuildOrder(4)]
public class HoldingEntity : IIdEntity, ISqlEntity
{
    [Required]
    public int Id { get; set; }
    [Required, Indexed]
    public int UserId { get; set; }
    [Required, Indexed]
    public int BeanId { get; set; }
    [Required, Indexed]
    public DateTime PurchaseDate { get; set; }
    [Required, Positive]
    public long Quantity { get; set; }
    [Required, Positive]
    public decimal Price { get; set; }

    [JsonIgnore]
    [Write(false)]
    public BeanEntity? Bean { get; set; }

    public override string ToString() => $"{PurchaseDate.ToShortDateString()} ({Quantity} @ {Price.ToCurrency(2)})";

    public HoldingEntity()
    {
        Id = 0;
        UserId = 0;
        BeanId = 0;
        PurchaseDate = default;
        Quantity = 0;
        Price = 0M;
        Bean = null;
    }

    [JsonIgnore]
    [Write(false)]
    public static string Sql => "create table Holdings(" +
      "Id integer constraint PkHolding primary key identity(1,1) not null, " +
      "UserId integer not null, " +
      "BeanId integer not null, " +
      "PurchaseDate datetime2 not null, " +
      "Quantity bigint default ((0)) not null, " +
      "Price decimal(20,2) default ((0)) not null, " +
      "Constraint FkHoldingUser foreign key (UserId) references Users(Id), " +
      "Constraint FkHoldingBean foreign key (BeanId) references Beans(Id) " +
      ");";
}
