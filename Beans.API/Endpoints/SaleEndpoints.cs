using Beans.Common;
using Beans.Services.Interfaces;

namespace Beans.API.Endpoints;

public static class SaleEndpoints
{
    public static void ConfigureSaleEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/Sale", Get).RequireAuthorization();
        app.MapGet("/api/v1/Sale/ById/{saleid}", ById).RequireAuthorization();
        app.MapGet("/api/v1/Sale/ForUser/{userid}", ForUser).RequireAuthorization();
    }

    private static async Task<IResult> Get(ISaleService saleService) => Results.Ok(await saleService.GetAsync());

    private static async Task<IResult> ById(string saleid, ISaleService saleService)
    {
        var model = await saleService.ReadAsync(saleid);
        if (model is null)
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.NotFound, "sale", "id", saleid)));
        }
        return Results.Ok(model);
    }

    private static async Task<IResult> ForUser(string userid, ISaleService saleService) => Results.Ok(await saleService.GetForUserAsync(userid));
}
