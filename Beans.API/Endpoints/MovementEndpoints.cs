using Beans.Common;
using Beans.Models;
using Beans.Services.Interfaces;

using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Options;

namespace Beans.API.Endpoints;

public static class MovementEndpoints
{
    public static void ConfigureMovementEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/Movement", Get);
        app.MapGet("/api/v1/Movement/ById/{movementid}", ById);
        app.MapGet("/api/v1/Movement/Ticker", Ticker);
        app.MapGet("/api/v1/Movement/ForBean/{beanid}", ForBean);
        app.MapGet("/api/v1/Movement/ForBean/{beanid}/{days}", DaysForBean);
        app.MapGet("/api/v1/Movement/Sigma/{beanid}/{days}", Sigma);
        app.MapGet("/api/v1/Movement/Top/{beanid}/{count}", Top);
        app.MapGet("/api/v1/Movement/HistoryById/{beanid}/{days}", HistoryById);
        app.MapGet("/api/v1/Movement/HistoryByName/{beanname}/{days}", HistoryByName);
        app.MapGet("/api/v1/Movement/MostRecent/{beanid}", MostRecent);
        app.MapPost("/api/v1/Movement/Catchup", Catchup);
        app.MapPost("/api/v1/Movement/Move", Move);
    }

    private static async Task<IResult> Get(IMovementService movementService) => Results.Ok(await movementService.GetAsync());

    private static async Task<IResult> ById(string movementid, IMovementService movementService)
    {
        var model = await movementService.ReadAsync(movementid);
        if (model is null)
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.NotFound, "movement", "id", movementid)));
        }
        return Results.Ok(model);
    }

    private static async Task<IResult> Ticker(IMovementService movementService) => Results.Ok(await movementService.MostRecentAsync());

    private static async Task<IResult> ForBean(string beanid, IMovementService movementService)
    {
        if (string.IsNullOrWhiteSpace(beanid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "movement id")));
        }
        var movements = await movementService.GetForBeanAsync(beanid);
        return Results.Ok(movements);
    }

    private static async Task<IResult> DaysForBean(string beanid, int days, IMovementService movementService)
    {
        if (string.IsNullOrWhiteSpace(beanid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "bean id")));
        }
        if (days <= 0)
        {
            return Results.BadRequest(new ApiError(String.Format(Strings.Invalid, "day count")));
        }
        var response = await movementService.GetForBeanAsync(beanid, days);
        return Results.Ok(response);
    }

    private static async Task<IResult> Sigma(string beanid, int days, IMovementService movementService)
    {
        if (string.IsNullOrWhiteSpace(beanid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "bean id")));
        }
        if (days <= 0)
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "number of days")));
        }
        return Results.Ok(await movementService.GetStandardDeviationAsync(beanid, days));
    }

    private static async Task<IResult> Top(string beanid, int count, IMovementService movementService)
    {
        if (string.IsNullOrWhiteSpace(beanid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "bean id")));
        }
        if (count <= 0)
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "movement count")));
        }
        var movements = await movementService.TopForBeanAsync(beanid, count);
        return Results.Ok(movements);
    }

    private static async Task<IResult> HistoryById(string beanid, int days, IMovementService movementService)
    {
        if (string.IsNullOrWhiteSpace(beanid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "bean id")));
        }
        var date = days == 0 ? default : DateTime.UtcNow.AddDays(-(days - 1));
        var movements = await movementService.HistoryAsync(beanid, date);
        return Results.Ok(movements);
    }

    private static async Task<IResult> HistoryByName(string beanname, int days, IMovementService movementService, IBeanService beanService)
    {
        if (string.IsNullOrWhiteSpace(beanname))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "bean name")));
        }
        var bean = await beanService.ReadForNameAsync(beanname);
        if (bean is null)
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.ItemNotFound, "bean", "name", beanname)));
        }
        var date = days == 0 ? default : DateTime.UtcNow.AddDays(-(days - 1));
        var movements = await movementService.HistoryAsync(bean.Id, date);
        return Results.Ok(movements);
    }

    private static async Task<IResult> MostRecent(string beanid, IMovementService movementService)
    {
        if (string.IsNullOrWhiteSpace(beanid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "bean id")));
        }
        var movement = await movementService.MostRecentAsync(beanid);
        return movement is null
          ? Results.BadRequest(new ApiError(string.Format(Strings.ItemNotFound, "bean", "id", beanid)))
          : Results.Ok(movement);
    }

    private static async Task<IResult> Catchup([FromBody] string key, IMovementService movementService, IBeanService beanService, IOptions<AppSettings> settings)
    {
        List<string> messages = new();
        if (key != settings.Value.ApiKey)
        {
            return Results.BadRequest(new ApiError(Strings.NotAuthorized));
        }
        var beanids = await beanService.BeanIdsAsync();
        var lowestDate = await movementService.LowestDateAsync();
        if (lowestDate == default)
        {
            return Results.Ok();
        }
        foreach (var beanid in beanids)
        {
            var result = await movementService.CatchupAsync(beanid, settings.Value.MinimumValue, lowestDate);
            if (!result.Successful)
            {
                messages.Add(result.ErrorMessage());
            }
        }
        if (messages.Any())
        {
            return Results.BadRequest(new ApiError(messages.ToArray()));
        }
        return Results.Ok();
    }

    private static async Task<IResult> Move([FromBody] string key, IMovementService movementService, IBeanService beanService, IOptions<AppSettings> settings)
    {
        List<string> messages = new();
        if (key != settings.Value.ApiKey)
        {
            return Results.BadRequest(new ApiError(Strings.NotAuthorized));
        }
        var beanids = await beanService.BeanIdsAsync();
        foreach (var beanid in beanids)
        {
            var result = await movementService.MoveAsync(beanid, settings.Value.MinimumValue, DateTime.UtcNow);
            if (!result.Successful)
            {
                messages.Add(result.ErrorMessage());
            }
        }
        if (messages.Any())
        {
            return Results.BadRequest(new ApiError(messages.ToArray()));
        }
        return Results.Ok();
    }
}