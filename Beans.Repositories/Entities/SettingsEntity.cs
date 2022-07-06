using Beans.Common;
using Beans.Common.Attributes;
using Beans.Common.Interfaces;

using Dapper.Contrib.Extensions;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Beans.Repositories.Entities;

[Table("Settings")]
[BuildOrder(1)]
public class SettingsEntity : ISqlEntity
{
    [Required, MaxLength(Constants.NameLength)]
    public string Name { get; set; }
    [Required]
    public string Value { get; set; }

    public SettingsEntity()
    {
        Name = string.Empty;
        Value = string.Empty;
    }

    public override string ToString() => Name;

    [JsonIgnore]
    [Write(false)]
    public static string Sql => "create table Settings (" +
      "[Name] nvarchar(50) constraint PkSettings primary key not null, " +
      "[Value] nvarchar(max) not null " +
      ");";
}
