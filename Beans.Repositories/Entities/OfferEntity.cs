using Beans.Common.Attributes;
using Beans.Common.Interfaces;

using Dapper.Contrib.Extensions;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Beans.Repositories.Entities;

[Table("Offers")]
[BuildOrder(7)]
public class OfferEntity : IIdEntity, ISqlEntity
{
    [Required]
    public int Id { get; set; }
    [Required, Indexed]
    public int BeanId { get; set; }
    [Required, Indexed]
    public int UserId { get; set; }
    [Required, Positive]
    public int HoldingId { get; set; }
    [Required, Positive]
    public long Quantity { get; set; }
    [Required, Positive]
    public decimal Price { get; set; }
    [Required]
    public bool Buy { get; set; }
    [Required, Indexed]
    public DateTime OfferDate { get; set; }

    [Write(false)]
    public BeanEntity? Bean { get; set; }

    [Write(false)]
    public UserEntity? User { get; set; }

    [Write(false)]
    public HoldingEntity? Holding { get; set; }

    [JsonIgnore]
    [Write(false)]
    public static string Sql => "create table Offers (" +
      "Id integer constraint PkOffer primary key identity(1,1) not null, " +
      "BeanId integer not null, " +
      "UserId integer not null, " +
      "HoldingId integer not null, " +
      "Quantity bigint default ((0)) not null, " +
      "Price decimal(20,2) default ((0)) not null, " +
      "Buy bit default ((0)) not null, " +
      "Offerdate datetime2 not null, " +
      "Constraint FkOfferBean foreign key (BeanId) references Beans(Id), " +
      "Constraint FkOfferUser foreign key (UserId) references Users(Id) " +
        // HoldingId is not a foreign key because it is 0 on buy offers
      ");";
}
