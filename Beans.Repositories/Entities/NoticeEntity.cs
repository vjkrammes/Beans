using Beans.Common;
using Beans.Common.Attributes;
using Beans.Common.Interfaces;

using Dapper.Contrib.Extensions;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Beans.Repositories.Entities;

[Table("Notices")]
[BuildOrder(6)]
public class NoticeEntity : IIdEntity, ISqlEntity
{
    [Required]
    public int Id { get; set; }
    [Required, Indexed]
    public int UserId { get; set; }
    [Required, Indexed]
    public int SenderId { get; set; }
    [Required, Indexed]
    public DateTime NoticeDate { get; set; }
    [Required, MaxLength(Constants.NameLength)]
    public string Title { get; set; }
    [Required]
    public string Text { get; set; }
    [Required]
    public bool Read { get; set; }

    [Write(false)]
    [JsonIgnore]
    public UserEntity? Sender { get; set; }

    public NoticeEntity()
    {
        Id = 0;
        UserId = 0;
        SenderId = 0;
        NoticeDate = DateTime.UtcNow;
        Title = string.Empty;
        Text = string.Empty;
        Read = false;
        Sender = null;
    }

    public override string ToString() => $"({NoticeDate.ToShortDateString()}) {Title.Beginning(25)}";

    [JsonIgnore]
    [Write(false)]
    public static string Sql => "create table Notices (" +
      "Id integer constraint PkNotice primary key identity (1,1) not null, " +
      "UserId integer not null, " +
      "SenderId integer not null, " +
      "NoticeDate datetime2 not null, " +
      "Title nvarchar(50) not null, " +
      "Text nvarchar(max) not null, " +
      "[Read] bit default ((0)) not null, " +
      "constraint FkNoticeUser foreign key (UserId) references Users(Id) " +
      ");";
    // notice that SenderId is not a foreign key, since -1 means sender is exchange, and -2 means sender is system
}
