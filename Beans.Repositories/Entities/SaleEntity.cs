using Beans.Common;
using Beans.Common.Attributes;
using Beans.Common.Interfaces;

using Dapper.Contrib.Extensions;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Beans.Repositories.Entities;

[Table("Sales")]
[BuildOrder(8)]
public class SaleEntity : IIdEntity, ISqlEntity
{
    [Required]
    public int Id { get; set; }
    [Required, Indexed]
    public int UserId { get; set; }
    [Required, Indexed]
    public int BeanId { get; set; }
    [Required, Indexed]
    public DateTime PurchaseDate { get; set; }
    [Required]
    public DateTime SaleDate { get; set; }
    [Required, Positive]
    public long Quantity { get; set; }
    [Required, Positive]
    public decimal CostBasis { get; set; }
    [Required, Positive]
    public decimal SalePrice { get; set; }

    [JsonIgnore]
    [Write(false)]
    public BeanEntity? Bean { get; set; }

    public override string ToString() => $"{Quantity} @ {SalePrice.ToCurrency(2)} on {SaleDate.ToShortDateString()}";

    [JsonIgnore]
    [Write(false)]
    public decimal GainOrLoss => (Quantity * SalePrice) - (Quantity * CostBasis);

    [JsonIgnore]
    [Write(false)]
    public static string Sql => "create table Sales (" +
      "Id integer constraint PkSale primary key identity (1,1) not null, " +
      "BeanId integer not null, " +
      "UserId integer not null, " +
      "PurchaseDate datetime2 not null, " +
      "SaleDate datetime2 not null, " +
      "Quantity bigint default ((0)) not null, " +
      "CostBasis decimal(20,2) default ((0)) not null, " +
      "SalePrice decimal(20,2) default ((0)) not null, " +
      "constraint FkSaleBean foreign key (BeanId) references Beans(Id), " +
      "constraint FkSaleUser foreign key (UserId) references Users(Id) " +
      ");";
}
