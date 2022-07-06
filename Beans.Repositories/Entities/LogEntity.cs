using Beans.Common;
using Beans.Common.Attributes;
using Beans.Common.Enumerations;
using Beans.Common.Interfaces;

using Dapper.Contrib.Extensions;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Beans.Repositories.Entities;

[Table("Logs")]
[BuildOrder(9)]
public class LogEntity : IIdEntity, ISqlEntity
{
    [Required]
    public int Id { get; set; }
    [Required, Indexed]
    public DateTime Timestamp { get; set; }
    [Required]
    public Level LogLevel { get; set; }
    [Required, MaxLength(Constants.NameLength)]
    public string Ip { get; set; }
    [Required, MaxLength(Constants.UriLength)]
    public string Identifier { get; set; }
    [Required, MaxLength(Constants.NameLength)]
    public string Source { get; set; }
    [Required]
    public string Description { get; set; }
    [Required]
    public string Data { get; set; }

    public LogEntity()
    {
        Id = 0;
        Timestamp = DateTime.UtcNow;
        LogLevel = Level.NoLevel;
        Ip = string.Empty;
        Identifier = string.Empty;
        Source = string.Empty;
        Description = string.Empty;
        Data = string.Empty;
    }

    public override string ToString() => $"{Timestamp} ({Source}) {Description.Beginning(25)}";

    [JsonIgnore]
    [Write(false)]
    public static string Sql => "create table Logs(" +
      "Id integer constraint PkLog primary key identity(1,1) not null, " +
      "Timestamp datetime2 not null, " +
      "Level integer default ((0)) not null, " +
      "Ip nvarchar(50) not null, " +
      "Identifier nvarchar(256) not null, " +
      "Source nvarchar(50) not null, " +
      "Description nvarchar(max) not null, " +
      "Data nvarchar(max) not null " +
      ");";
}
