using Beans.Common;
using Beans.Services.Interfaces;

namespace Beans.API.Endpoints;

public static class NoticeEndpoints
{
    public static void ConfigureNoticeEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/Notice", Get);
        app.MapGet("/api/v1/Notice/ById/{noticeid}", ById);
        app.MapGet("/api/v1/Notice/{userid}", Notices).RequireAuthorization();
        app.MapGet("/api/v1/Notice/Unread/{userid}", Unread).RequireAuthorization();
        app.MapGet("/api/v1/Notice/UnreadCount/{userid}", UnreadCount);
        app.MapPut("/api/v1/Notice/MarkRead/{noticeid}", MarkRead).RequireAuthorization();
        app.MapPut("/api/v1/Notice/MarkAllRead/{userid}", MarkAllRead).RequireAuthorization();
        app.MapDelete("/api/v1/Notice/Delete/{noticeid}", Delete).RequireAuthorization();
        app.MapDelete("/api/v1/Notice/DeleteAll/{userid}", DeleteAll).RequireAuthorization();
    }

    private static async Task<IResult> Get(INoticeService noticeService) => Results.Ok(await noticeService.GetAsync());

    private static async Task<IResult> ById(string noticeid, INoticeService noticeService)
    {
        var model = await noticeService.ReadAsync(noticeid);
        if (model is null)
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.NotFound, "notice", "id", noticeid)));
        }
        return Results.Ok(model);
    }

    private static async Task<IResult> Notices(string userid, INoticeService noticeService)
    {
        if (string.IsNullOrWhiteSpace(userid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "user id")));
        }
        var notices = await noticeService.GetForUserAsync(userid);
        return Results.Ok(notices);
    }

    private static async Task<IResult> Unread(string userid, INoticeService noticeService)
    {
        if (string.IsNullOrWhiteSpace(userid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "user id")));
        }
        var notices = await noticeService.Unread(userid);
        return Results.Ok(notices);
    }

    private static async Task<IResult> UnreadCount(string userid, INoticeService noticeService) => 
        Results.Ok(await noticeService.GetUnreadNoticeCountAsync(userid));

    private static async Task<IResult> MarkRead(string noticeid, INoticeService noticeService)
    {
        if (string.IsNullOrWhiteSpace(noticeid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "notice id")));
        }
        var result = await noticeService.MarkReadAsync(noticeid);
        if (result.Successful)
        {
            return Results.Ok();
        }
        return Results.BadRequest(new ApiError(result.ErrorMessage()));
    }

    private static async Task<IResult> MarkAllRead(string userid, INoticeService noticeService)
    {
        if (string.IsNullOrWhiteSpace(userid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "user id")));
        }
        var result = await noticeService.MarkAllReadAsync(userid);
        if (result.Successful)
        {
            return Results.Ok();
        }
        return Results.BadRequest(new ApiError(result.ErrorMessage()));
    }

    private static async Task<IResult> Delete(string noticeid, INoticeService noticeService)
    {
        if (string.IsNullOrWhiteSpace(noticeid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "notice id")));
        }
        var notice = await noticeService.ReadAsync(noticeid);
        if (notice is null)
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.ItemNotFound, "notice", "id", noticeid)));
        }
        var result = await noticeService.DeleteAsync(notice);
        if (result.Successful)
        {
            return Results.Ok();
        }
        return Results.BadRequest(new ApiError(result.ErrorMessage()));
    }

    private static async Task<IResult> DeleteAll(string userid, INoticeService noticeService)
    {
        if (string.IsNullOrWhiteSpace(userid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "user id")));
        }
        var result = await noticeService.DeleteAllAsync(userid);
        if (result.Successful)
        {
            return Results.Ok();
        }
        return Results.BadRequest(new ApiError(result.ErrorMessage()));
    }
}
