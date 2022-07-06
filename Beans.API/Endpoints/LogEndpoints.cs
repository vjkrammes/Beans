using Beans.Common;
using Beans.Services.Interfaces;

namespace Beans.API.Endpoints;

public static class LogEndpoints
{
    public static void ConfigureLogEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/Log", Get).RequireAuthorization(Constants.ADMIN_REQUIRED);
        app.MapGet("/api/v1/Log/ById/{logid}", ById).RequireAuthorization(Constants.ADMIN_REQUIRED);
    }

    private static async Task<IResult> Get(ILogService logService) => Results.Ok(await logService.GetAsync());

    private static async Task<IResult> ById(string logid, ILogService logService)
    {
        var model = await logService.ReadAsync(logid);
        if (model is null)
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.NotFound, "log", "id", logid)));
        }
        return Results.Ok(model);
    }
}
