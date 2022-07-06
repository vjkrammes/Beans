using Beans.Common;
using Beans.Models;

namespace Beans.Services.Interfaces;
public interface INoticeService : IDataService<NoticeModel>
{
    Task<IEnumerable<NoticeModel>> GetForUserAsync(string userid);
    Task<IEnumerable<NoticeModel>> GetForSenderAsync(string userid);
    Task<IEnumerable<NoticeModel>> GetForUserAndSenderAsync(string userid, string senderid);
    Task<IEnumerable<NoticeModel>> Unread(string userid);
    Task<bool> UserHasNoticesAsync(string userid);
    Task<ApiError> SendNoticeAsync(string userid, string senderid, string title, params string[] messages);
    Task<ApiError> MarkReadAsync(string noticeid);
    Task<ApiError> MarkAllReadAsync(string userid);
    Task<ApiError> DeleteAllAsync(string userid);
    Task<int> GetUnreadNoticeCountAsync(string userid);
}
