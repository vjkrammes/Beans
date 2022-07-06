using Beans.Common;
using Beans.Common.Attributes;
using Beans.Common.Interfaces;

using Dapper.Contrib.Extensions;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Beans.Repositories.Entities;

[Table("Beans")]
[BuildOrder(3)]
public class BeanEntity : IIdEntity, ISqlEntity
{
    [Required]
    public int Id { get; set; }
    [Required, MaxLength(Constants.NameLength)]
    [Indexed]
    public string Name { get; set; }
    [Required]
    public long ARGB { get; set; }
    [Required, NonNegative]
    public decimal Price { get; set; }
    [Required, NonNegative]
    public long Quantity { get; set; }      // total amount of this bean extant
    [Required, NonNegative]
    public long Held { get; set; }          // total number of beans held by users
    [Required, NonNegative]
    public long ExchangeHeld { get; set; }  // total number of beans held by the exchange
    [Required, MaxLength(Constants.UriLength)]
    public string Filename { get; set; }

    public override string ToString() => Name;

    public BeanEntity()
    {
        Id = 0;
        Name = string.Empty;
        ARGB = 0xFF000000;
        Price = 0M;
        Quantity = 0;
        Held = 0;
        ExchangeHeld = 0;
        Filename = string.Empty;
    }

    [JsonIgnore]
    [Write(false)]
    public decimal Valuation => Price * Held;

    [JsonIgnore]
    [Write(false)]
    public static string Sql => "create table Beans(" +
      "Id integer constraint PkBean primary key identity(1,1) not null, " +
      "Name nvarchar(50) not null, " +
      "ARGB bigint default ((0)) not null, " +
      "Price decimal(20,2) default ((0)) not null, " +
      "Quantity bigint default ((0)) not null, " +
      "Held bigint default ((0)) not null, " +
      "ExchangeHeld bigint default ((0)) not null, " +
      "Filename nvarchar(256) not null, " +
      "Constraint UniqueBeanName unique nonclustered (Name asc), " +
      "Constraint UniqueFilename unique nonclustered (Filename asc) " +
      ");";
}
