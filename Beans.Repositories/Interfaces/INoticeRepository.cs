
using Beans.Common;
using Beans.Repositories.Entities;

namespace Beans.Repositories.Interfaces;
public interface INoticeRepository : IRepository<NoticeEntity>
{
    Task<IEnumerable<NoticeEntity>> GetForUserAsync(int userid);
    Task<IEnumerable<NoticeEntity>> GetForSenderAsync(int userid);
    Task<IEnumerable<NoticeEntity>> GetForUserAndSenderAsync(int userid, int senderid);
    Task<bool> UserHasNoticesAsync(int userid);
    Task<DalResult> SendNoticeAsync(int userid, int senderid, string title, params string[] messages);
    Task<DalResult> MarkReadAsync(int noticeid);
    Task<DalResult> MarkAllReadAsync(int userid);
    Task<DalResult> DeleteAllAsync(int userid);
    Task<int> GetUnreadNoticeCountAsync(int userid);
}
