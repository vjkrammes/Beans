using Beans.Common;
using Beans.Common.Attributes;
using Beans.Common.Interfaces;

using Dapper.Contrib.Extensions;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Beans.Repositories.Entities;

[Table("Users")]
[BuildOrder(2)]
public class UserEntity : IIdEntity, ISqlEntity
{
    [Required]
    public int Id { get; set; }
    [Required, MaxLength(Constants.UriLength)]
    [Indexed]
    public string Identifier { get; set; }
    [Required, MaxLength(Constants.UriLength)]
    [Indexed]
    public string Email { get; set; }
    [Required, MaxLength(Constants.NameLength)]
    public string FirstName { get; set; }
    [Required, MaxLength(Constants.NameLength)]
    public string LastName { get; set; }
    [Required, MaxLength(Constants.NameLength)]
    public string DisplayName { get; set; }
    [Required, NonNegative]
    public decimal Balance { get; set; }
    [Required, NonNegative]
    public decimal OwedToExchange { get; set; }
    [Required]
    public DateTime DateJoined { get; set; }
    [Required]
    public bool IsAdmin { get; set; }

    public override string ToString() => DisplayName;

    public UserEntity()
    {
        Id = 0;
        Identifier = string.Empty;
        Email = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        DisplayName = string.Empty;
        Balance = 0M;
        OwedToExchange = 0M;
        DateJoined = default;
        IsAdmin = false;
    }

    [JsonIgnore]
    [Write(false)]
    public static string Sql => "create table Users (" +
      "Id integer constraint PkUser primary key identity(1,1) not null, " +
      "Identifier nvarchar(256) not null, " +
      "Email nvarchar(256) not null, " +
      "FirstName nvarchar(50) not null, " +
      "LastName nvarchar(50) not null, " +
      "DisplayName nvarchar(50) not null, " +
      "Balance decimal(20,2) default ((0)) not null, " +
      "OwedToExchange decimal(20,2) default ((0)) not null, " +
      "DateJoined datetime2 not null, " +
      "IsAdmin bit default ((0)) not null, " +
      "constraint UniqueEmail unique nonclustered (Email asc), " +
      "constraint UniqueDisplayName unique nonclustered (DisplayName asc) " +
      ");";
}
