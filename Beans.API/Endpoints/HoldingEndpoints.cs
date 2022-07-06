using Beans.API.Infrastructure;
using Beans.API.Models;
using Beans.Common;
using Beans.Models;
using Beans.Services.Interfaces;

using Microsoft.AspNetCore.Mvc;

namespace Beans.API.Endpoints;

public static class HoldingEndpoints
{
    public static void ConfigureHoldingEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/Holding", Get);
        app.MapGet("/api/v1/Holding/Holdings/{userid}", GetHoldings).RequireAuthorization();
        app.MapGet("/api/v1/Holding/ById/{holdingid}", ById);
        app.MapGet("/api/v1/Holding/Held/{beanid}", PrivatelyHeld);
        app.MapGet("/api/v1/Holding/ForUser/{userid}", ForUser).RequireAuthorization();
        app.MapGet("/api/v1/Holding/ForUser/{userid}/{beanid}", ForUserAndBean).RequireAuthorization();
        app.MapGet("/api/v1/Holding/Balance/{userid}", Balance).RequireAuthorization();
        app.MapGet("/api/v1/Holding/Basis/{userid}", Basis).RequireAuthorization();
        app.MapGet("/api/v1/Holding/CostBases/{userid}", Bases).RequireAuthorization();
        app.MapGet("/api/v1/Holding/Count/{userid}/{beanid?}", Count).RequireAuthorization();
        app.MapPost("/api/v1/Holding/Search", Search).RequireAuthorization(); // should be get but can't have body in a get
        app.MapPost("/api/v1/Holding/Reset", Reset).RequireAuthorization(Constants.ADMIN_REQUIRED);
    }

    private static async Task<IResult> Get(IHoldingService holdingService) => Results.Ok(await holdingService.GetAsync());

    private static async Task<IResult> GetHoldings(string userid, IHoldingService holdingService, IBeanService beanService)
    {
        var ret = new List<UserHoldingModel>();
        var beans = await beanService.GetAsync();
        foreach (var bean in beans)
        {
            ret.Add(new()
            {
                Id = bean.Id,
                Name = bean.Name,
                Filename = bean.Filename,
                Held = 0,
                Quantity = 0
            });
        }
        var holdings = await holdingService.GetForUserAsync(userid);
        if (holdings is null || !holdings.Any())
        {
            return Results.Ok(ret);
        }
        foreach (var holding in holdings)
        {
            var existing = ret.SingleOrDefault(x => x.Id == holding.BeanId);
            if (existing is null)
            {
                ret.Add(new()
                {
                    Id = holding.BeanId,
                    Name = (await beanService.ReadAsync(holding.BeanId))?.Name ?? string.Empty,
                    Held = holding.Quantity
                });
            }
            else
            {
                existing.Held += holding.Quantity;
            }
        }
        return Results.Ok(ret);
    }

    private static async Task<IResult> ById(string holdingid, IHoldingService holdingService)
    {
        var model = await holdingService.ReadAsync(holdingid);
        if (model is null)
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.NotFound, "holding", "id", holdingid)));
        }
        return Results.Ok(model);
    }

    private static async Task<IResult> PrivatelyHeld(string beanid, IHoldingService holdingService)
    {
        if (string.IsNullOrWhiteSpace(beanid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "bean id")));
        }
        var held = await holdingService.BeansHeldByBeanAsync(beanid);
        return Results.Ok(held);
    }

    private static async Task<IResult> ForUser(string userid, IHoldingService holdingService)
    {
        if (string.IsNullOrWhiteSpace(userid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "user id")));
        }
        var holdings = await holdingService.GetForUserAsync(userid);
        return Results.Ok(holdings);
    }

    private static async Task<IResult> ForUserAndBean(string userid, string beanid, IHoldingService holdingService)
    {
        if (string.IsNullOrWhiteSpace(userid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "user id")));
        }
        if (string.IsNullOrWhiteSpace(beanid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "bean id")));
        }
        var holdings = await holdingService.GetForBeanAsync(userid, beanid);
        return Results.Ok(holdings);
    }

    private static async Task<IResult> Balance(string userid, IHoldingService holdingService)
    {
        if (string.IsNullOrWhiteSpace(userid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "user id")));
        }
        var holdings = await holdingService.GetForUserAsync(userid);
        if (holdings is null || !holdings.Any())
        {
            return Results.Ok(0M);
        }
        return Results.Ok(holdings.Sum(x => x.Quantity * (x.Bean?.Price ?? 0)));
    }

    private static async Task<IResult> Basis(string userid, IHoldingService holdingService)
    {
        if (string.IsNullOrWhiteSpace(userid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "user id")));
        }
        var holdings = await holdingService.GetForUserAsync(userid);
        if (holdings is null || !holdings.Any())
        {
            return Results.Ok(0M);
        }
        return Results.Ok(holdings.Sum(x => x.Quantity * x.Price));
    }

    private static async Task<IResult> Bases(string userid, IHoldingService holdingService)
    {
        if (string.IsNullOrWhiteSpace(userid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Required, "user id")));
        }
        var ret = await holdingService.GetCostBasesAsync(userid);
        if (ret is null)
        {
            return Results.Ok(Array.Empty<CostBasis>());
        }
        return Results.Ok(ret);
    }

    private static async Task<IResult> Count(string userid, string? beanid, IHoldingService holdingService)
    {
        if (string.IsNullOrWhiteSpace(userid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "user id")));
        }
        return Results.Ok(await holdingService.HoldingCountAsync(userid, beanid));
    }

    private static async Task<IResult> Search([FromBody] SearchHoldingsModel model, IHoldingService holdingService)
    {
        if (model is null)
        {
            return Results.BadRequest(new ApiError(Strings.InvalidModel));
        }
        return Results.Ok(await holdingService.SearchAsync(model));
    }

    private static async Task<IResult> Reset(IHttpContextAccessor accessor, IHoldingService holdingService, IUserService userService)
    {
        var context = accessor?.HttpContext;
        if (context is null)
        {
            return Results.BadRequest(new ApiError(Strings.NotAuthenticated));
        }
        var token = context.GetToken();
        if (token is null)
        {
            return Results.BadRequest(new ApiError(Strings.NotAuthenticated));
        }
        var identifier = token.Claims.SingleOrDefault(x => x.Type == "sub")?.Value;
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return Results.BadRequest(new ApiError(Strings.NotAuthenticated));
        }
        var user = await userService.ReadForIdentifierAsync(identifier);
        if (user is null || !user.IsAdmin)
        {
            return Results.BadRequest(new ApiError(Strings.NotAuthorized));
        }
        var result = await holdingService.ResetHoldingsAsync();
        if (result.Successful)
        {
            return Results.Ok();
        }
        return Results.BadRequest(result);
    }
}
