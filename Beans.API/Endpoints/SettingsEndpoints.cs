using Beans.Common;
using Beans.Services.Interfaces;

namespace Beans.API.Endpoints;

public static class SettingsEndpoints
{
    public static void ConfigureSettingsEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/Settings", Get);
        app.MapGet("/api/v1/Settings/{name}", Read);
    }

    private static async Task<IResult> Get(ISettingsService settingsService) => Results.Ok(await settingsService.GetAsync());

    private static async Task<IResult> Read(string name, ISettingsService settingsService)
    {
        var model = await settingsService.ReadAsync(name);
        if (model is null)
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.NotFound, "setting", "key", name)));
        }
        return Results.Ok(model);
    }
}
