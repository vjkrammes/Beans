using Beans.API.Models;
using Beans.Common;
using Beans.Models;
using Beans.Services.Interfaces;

using Microsoft.AspNetCore.Mvc;

namespace Beans.API.Endpoints;

public static class BeanEndpoints
{
    public static void ConfigureBeanEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/Bean", Get);
        app.MapGet("/api/v1/Bean/Ids", BeanIds);
        app.MapGet("/api/v1/Bean/ById/{beanid}", ById);
        app.MapGet("/api/v1/Bean/ByName/{name}", ByName);
        app.MapGet("/api/v1/Bean/UserHistory/{userid}/{days?}", UserHistory).RequireAuthorization();
        app.MapGet("/api/v1/Bean/BeanHistory/{beanid}/{days?}", BeanHistory);
        app.MapGet("/api/v1/Bean/AllBeanHistory/{days?}", AllBeanHistory);
        app.MapPost("/api/v1/Bean/Sell/{holdingid}/{quantity}", SellToExchange).RequireAuthorization();
        app.MapPost("/api/v1/Bean/Buy/{userid}/{beanid}/{quantity}", BuyFromExchange).RequireAuthorization();
        app.MapPost("/api/v1/Bean/BuyBeans", BuyBeans).RequireAuthorization();
        app.MapPost("/api/v1/Bean/SellBeans", SellBeans).RequireAuthorization();
    }

    private static async Task<IResult> Get(IBeanService beanService) => Results.Ok(await beanService.GetAsync());

    private static async Task<IResult> BeanIds(IBeanService beanService) => Results.Ok(await beanService.BeanIdsAsync());

    private static async Task<IResult> ById(string beanid, IBeanService beanService)
    {
        var model = await beanService.ReadAsync(beanid);
        if (model is null)
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.NotFound, "bean", "id", beanid)));
        }
        return Results.Ok(model);
    }

    private static async Task<IResult> ByName(string name, IBeanService beanService)
    {
        var model = await beanService.ReadForNameAsync(name);
        if (model is null)
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.NotFound, "bean", "name", name)));
        }
        return Results.Ok(model);
    }

    private static async Task<IResult> UserHistory(string userid, int? days, IBeanService beanService, [FromServices] IHoldingService holdingService)
    {
        if (string.IsNullOrWhiteSpace(userid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "user id")));
        }
        if (!days.HasValue)
        {
            days = int.MaxValue;
        }
        var ids = await beanService.BeanIdsAsync(userid);
        List<BeanHistoryModel> ret = new();
        foreach (var id in ids)
        {
            var history = await beanService.HistoryAsync(id, days.Value);
            if (history is not null)
            {
                history.Basis = await holdingService.GetCostBasisAsync(userid, id);
                history.Quantity = (await holdingService.SummaryAsync(userid, history.BeanId))?.Quantity ?? 0;
                ret.Add(history);
            }
        }
        return Results.Ok(ret);
    }

    private static async Task<IResult> AllBeanHistory(int? days, IBeanService beanService)
    {
        if (!days.HasValue)
        {
            days = int.MaxValue;
        }
        var ids = await beanService.BeanIdsAsync();
        List<BeanHistoryModel> ret = new();
        foreach (var id in ids)
        {
            var history = await beanService.HistoryAsync(id, days.Value);
            if (history is not null)
            {
                ret.Add(history);
            }
        }
        return Results.Ok(ret);
    }

    private static async Task<IResult> BeanHistory(string beanid, int? days, IBeanService beanService)
    {
        if (!days.HasValue)
        {
            days = int.MaxValue;
        }
        var history = await beanService.HistoryAsync(beanid, days.Value);
        if (history is null)
        {
            return Results.BadRequest(new ApiError(Strings.NoHistory));
        }
        return Results.Ok(history);
    }

    private static async Task<IResult> SellToExchange(string holdingid, long quantity, IBeanService beanService)
    {
        var result = await beanService.SellToExchangeAsync(holdingid, quantity);
        if (result.Successful)
        {
            return Results.Ok();
        }
        return Results.BadRequest(result);
    }

    private static async Task<IResult> BuyFromExchange(string userid, string beanid, long quantity, IBeanService beanService)
    {
        var result = await beanService.BuyFromExchangeAsync(userid, beanid, quantity);
        if (result.Successful)
        {
            return Results.Ok();
        }
        return Results.BadRequest(result);
    }

    private static async Task<IResult> BuyBeans(BuyBeanModel model, IBeanService beanService)
    {
        var ret = new List<BuySellResult>();
        if (model is null || string.IsNullOrWhiteSpace(model.Userid))
        {
            ret.Add(new() { Color = "All", Result = "Invalid model passed"} );
            return Results.Ok(ret.ToArray());
        }
        if (model.Items is null || !model.Items.Any())
        {
            return Results.Ok(ret.ToArray());
        }
        foreach (var item in model.Items)
        {
            var bean = await beanService.ReadAsync(item.Id);
            if (bean is null)
            {
                return Results.BadRequest(new ApiError(string.Format(Strings.NotFound, "bean", "id", item.Id)));
            }
            var result = await beanService.BuyFromExchangeAsync(model.Userid, item.Id, item.Quantity);
            if (result.Successful)
            {
                ret.Add(new(){ Color = bean.Name, Result = "Success"});
            }
            else
            {
                ret.Add(new() { Color = bean.Name, Result = result.Message });
            }
        }
        return Results.Ok(ret.ToArray());
    }

    private static async Task<IResult> SellBeans(SellBeanModel model, IBeanService beanService, IHoldingService holdingService)
    {
        var ret = new List<BuySellResult>();
        if (model is null || string.IsNullOrWhiteSpace(model.Userid))
        {
            ret.Add(new() { Color = "All", Result = "Invalid model passed" });
            return Results.Ok(ret.ToArray());
        }
        if (model.Holdings is null || !model.Holdings.Any())
        {
            return Results.Ok(ret);
        }
        foreach (var holding in model.Holdings)
        {
            var h = await holdingService.ReadAsync(holding.HoldingId);
            if (h is null)
            {
                ret.Add(new() { Color = "Unknown", Result = $"No Holding found for Id '{holding.HoldingId}'" });
                continue;
            }
            var result = await beanService.SellToExchangeAsync(holding.HoldingId, holding.Quantity);
            var r = new BuySellResult
            {
                Color = h.Bean!.Name,
                Result = result.Successful ? "Success" : result.Message
            };
            ret.Add(r);
        }
        return Results.Ok(ret.ToArray());
    }
}
