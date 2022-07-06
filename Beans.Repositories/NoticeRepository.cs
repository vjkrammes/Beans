using Beans.Common;
using Beans.Repositories.Entities;
using Beans.Repositories.Interfaces;

using Dapper;

using System.Data.SqlClient;
using System.Text;

namespace Beans.Repositories;
public class NoticeRepository : RepositoryBase<NoticeEntity>, INoticeRepository
{
    private readonly IUserRepository _userRepository;

    public NoticeRepository(IDatabase database, IUserRepository userRepository) : base(database) => _userRepository = userRepository;

    private async Task<IEnumerable<NoticeEntity>> GetNoticesAsync(string sql)
    {
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            var ret =  await conn.QueryAsync<NoticeEntity>(sql);
            if (ret is not null && ret.Any())
            {
                foreach (var entity in ret)
                {
                    entity.Sender = entity.SenderId > 0 ? await _userRepository.ReadAsync(entity.SenderId) : null;
                }
            }
            return ret!;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<IEnumerable<NoticeEntity>> GetForUserAsync(int userid)
    {
        var sql = $"select * from Notices where UserId={userid} order by NoticeDate desc;";
        return await GetNoticesAsync(sql);
    }

    public async Task<IEnumerable<NoticeEntity>> GetForSenderAsync(int userid)
    {
        var sql = $"select * from Notices where SenderId={userid} order by NoticeDate desc;";
        return await GetNoticesAsync(sql);
    }

    public async Task<IEnumerable<NoticeEntity>> GetForUserAndSenderAsync(int userid, int senderid)
    {
        var sql = $"select * from Notices where UserId={userid} and SenderId={senderid} order by NoticeDate desc;";
        return await GetNoticesAsync(sql);
    }

    public async Task<bool> UserHasNoticesAsync(int userid)
    {
        if (userid <= 0)
        {
            return false;
        }
        var sql = $"Select count(*) from Notices where UserId={userid} or SenderId={userid};";
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            var count = await conn.QueryFirstOrDefaultAsync<int>(sql);
            return count > 0;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<DalResult> SendNoticeAsync(int userid, int senderid, string title, params string[] messages)
    {
        var entity = new NoticeEntity
        {
            Id = 0,
            UserId = userid,
            SenderId = senderid,
            NoticeDate = DateTime.UtcNow,
            Title = title,
            Text = string.Empty,
            Read = false
        };
        if (messages is not null && messages.Any())
        {
            var sb = new StringBuilder();
            messages.ForEach(x => sb.AppendLine(x));
            entity.Text = sb.ToString();
        }
        return await InsertAsync(entity);
    }

    private async Task<DalResult> ExecuteCommand(string sql)
    {
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            await conn.ExecuteAsync(sql);
            return DalResult.Success;
        }
        catch (Exception ex)
        {
            return DalResult.FromException(ex);
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<DalResult> MarkReadAsync(int noticeid)
    {
        var sql = $"update Notices set [Read]=1 where Id={noticeid};";
        return await ExecuteCommand(sql);
    }

    public async Task<DalResult> MarkAllReadAsync(int userid)
    {
        var sql = $"update Notices set [Read]=1 where UserId={userid};";
        return await ExecuteCommand(sql);
    }

    public async Task<DalResult> DeleteAllAsync(int userid)
    {
        var sql = $"Delete from Notices where UserId={userid};";
        return await ExecuteCommand(sql);
    }

    public async Task<int> GetUnreadNoticeCountAsync(int userid)
    {
        if (userid <= 0)
        {
            return 0;
        }
        var sql = $"Select count(*) from Notices where UserId={userid} and [Read]=0;";
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            var ret = await conn.ExecuteScalarAsync<int>(sql);
            return ret;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }
}
