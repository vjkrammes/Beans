using Beans.Common;
using Beans.Models;
using Beans.Repositories.Entities;
using Beans.Repositories.Interfaces;
using Beans.Services.Interfaces;

namespace Beans.Services;
public class NoticeService : INoticeService
{
    private readonly INoticeRepository _noticeRepository;
    private readonly IUserRepository _userRepository;

    public NoticeService(INoticeRepository noticeRepository, IUserRepository userRepository)
    {
        _noticeRepository = noticeRepository;
        _userRepository = userRepository;
    }

    public async Task<int> CountAsync() => await _noticeRepository.CountAsync();

    public async Task<ApiError> ValidateModelAsync(NoticeModel model, bool checkid = false)
    {
        if (model is null || IdEncoder.DecodeId(model.UserId) <= 0 || IdEncoder.DecodeId(model.SenderId) < -1 || string.IsNullOrWhiteSpace(model.Title))
        {
            return new(Strings.InvalidModel);
        }
        var user = await _userRepository.ReadAsync(model.UserId);
        if (user is null)
        {
            return new(string.Format(Strings.NotFound, "user", "id", model.UserId));
        }
        if (IdEncoder.DecodeId(model.SenderId) > 0)
        {
            var sender = await _userRepository.ReadAsync(model.SenderId);
            if (sender is null)
            {
                return new(string.Format(Strings.NotFound, "user", "id", model.SenderId));
            }
        }
        if (model.NoticeDate == default)
        {
            model.NoticeDate = DateTime.UtcNow;
        }
        if (string.IsNullOrWhiteSpace(model.Id))
        {
            model.UserId = IdEncoder.EncodeId(0);
        }
        if (checkid && IdEncoder.DecodeId(model.Id) == 0)
        {
            return new(string.Format(Strings.Invalid, "id"));
        }
        if (string.IsNullOrEmpty(model.Text))
        {
            model.Text = "(No notice text)";
        }
        return ApiError.Success;
    }

    public async Task<ApiError> InsertAsync(NoticeModel model)
    {
        var checkresult = await ValidateModelAsync(model);
        if (!checkresult.Successful)
        {
            return checkresult;
        }
        NoticeEntity entity = model!;
        try
        {
            var result = await _noticeRepository.InsertAsync(entity);
            if (result.Successful)
            {
                model.Id = IdEncoder.EncodeId(entity.Id);
            }
            return ApiError.FromDalResult(result);
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    public async Task<ApiError> UpdateAsync(NoticeModel model)
    {
        var checkresult = await ValidateModelAsync(model, true);
        if (!checkresult.Successful)
        {
            return checkresult;
        }
        NoticeEntity entity = model!;
        try
        {
            return ApiError.FromDalResult(await _noticeRepository.UpdateAsync(entity));
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    public async Task<ApiError> DeleteAsync(NoticeModel model)
    {
        if (model is null)
        {
            return new(Strings.InvalidModel);
        }
        try
        {
            return ApiError.FromDalResult(await _noticeRepository.DeleteAsync(IdEncoder.DecodeId(model.Id)));
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    private static IEnumerable<NoticeModel> Finish(IEnumerable<NoticeEntity> entities)
    {
        var models = entities.ToModels<NoticeModel, NoticeEntity>();
        models.ForEach(x => x.CanDelete = true);
        return models;
    }

    public async Task<IEnumerable<NoticeModel>> GetAsync()
    {
        var entities = await _noticeRepository.GetAsync();
        return Finish(entities);
    }

    public async Task<IEnumerable<NoticeModel>> GetForUserAsync(string userid)
    {
        var entities = await _noticeRepository.GetForUserAsync(IdEncoder.DecodeId(userid));
        return Finish(entities);
    }

    public async Task<IEnumerable<NoticeModel>> GetForSenderAsync(string userid)
    {
        var entities = await _noticeRepository.GetForSenderAsync(IdEncoder.DecodeId(userid));
        return Finish(entities);
    }

    public async Task<IEnumerable<NoticeModel>> GetForUserAndSenderAsync(string userid, string senderid)
    {
        var entities = await _noticeRepository.GetForUserAndSenderAsync(IdEncoder.DecodeId(userid), IdEncoder.DecodeId(senderid));
        return Finish(entities);
    }

    public async Task<IEnumerable<NoticeModel>> Unread(string userid)
    {
        var sql = $"select * from Notices where UserId={IdEncoder.DecodeId(userid)} and [Read]=0 order by NoticeDate desc;";
        var entities = await _noticeRepository.GetAsync(sql);
        return Finish(entities);
    }

    public async Task<NoticeModel?> ReadAsync(string id)
    {
        var entity = await _noticeRepository.ReadAsync(IdEncoder.DecodeId(id));
        if (entity is not null)
        {
            NoticeModel model = entity!;
            model.CanDelete = true;
            return model;
        }
        return null;
    }

    public async Task<bool> UserHasNoticesAsync(string userid) => await _noticeRepository.UserHasNoticesAsync(IdEncoder.DecodeId(userid));

    public async Task<ApiError> SendNoticeAsync(string userid, string senderid, string title, params string[] messages)
    {
        try
        {
            return ApiError.FromDalResult(await _noticeRepository.SendNoticeAsync(IdEncoder.DecodeId(userid), IdEncoder.DecodeId(senderid), title, messages));
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    public async Task<ApiError> MarkReadAsync(string noticeid)
    {
        try
        {
            return ApiError.FromDalResult(await _noticeRepository.MarkReadAsync(IdEncoder.DecodeId(noticeid)));
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    public async Task<ApiError> MarkAllReadAsync(string userid)
    {
        try
        {
            return ApiError.FromDalResult(await _noticeRepository.MarkAllReadAsync(IdEncoder.DecodeId(userid)));
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    public async Task<ApiError> DeleteAllAsync(string userid)
    {
        try
        {
            return ApiError.FromDalResult(await _noticeRepository.DeleteAllAsync(IdEncoder.DecodeId(userid)));
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    public async Task<int> GetUnreadNoticeCountAsync(string userid) => await _noticeRepository.GetUnreadNoticeCountAsync(IdEncoder.DecodeId(userid));
}
