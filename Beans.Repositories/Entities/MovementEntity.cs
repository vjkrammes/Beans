using Beans.Common;
using Beans.Common.Attributes;
using Beans.Common.Enumerations;
using Beans.Common.Interfaces;

using Dapper.Contrib.Extensions;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Beans.Repositories.Entities;

[Table("Movements")]
[BuildOrder(5)]
public class MovementEntity : IIdEntity, ISqlEntity
{
    [Required]
    public int Id { get; set; }
    [Required, Indexed]
    public int BeanId { get; set; }
    [Required, Indexed]
    public DateTime MovementDate { get; set; }
    [Required, Positive]
    public decimal Open { get; set; }
    [Required, Positive]
    public decimal Close { get; set; }
    [Required]
    public decimal Movement { get; set; }
    [Required]
    public MovementType MovementType { get; set; }

    [JsonIgnore]
    [Write(false)]
    public BeanEntity? Bean { get; set; }

    public override string ToString() =>
      $"{MovementDate.ToShortDateString()}: Open: {Open.ToCurrency(2)}, Close: {Close.ToCurrency(2)}: {Movement.ToCurrency(2)}";

    [JsonIgnore]
    [Write(false)]
    public static string Sql => "create table Movements(" +
      "Id integer constraint PkMovement primary key identity(1,1) not null, " +
      "BeanId integer not null, " +
      "MovementDate date not null, " +
      "[Open] decimal(20,2) default ((0)) not null, " +
      "[Close] decimal(20,2) default ((0)) not null, " +
      "Movement decimal(20,2) default ((0)) not null, " +
      "MovementType integer not null, " +
      "Constraint UniqueMovement unique nonclustered (BeanId asc, MovementDate asc), " +
      "Constraint FkMovementBean foreign key (BeanId) references Beans(Id) " +
      ");";
}
