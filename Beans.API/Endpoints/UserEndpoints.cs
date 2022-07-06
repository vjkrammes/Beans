using Beans.API.Infrastructure;
using Beans.API.Models;
using Beans.Common;
using Beans.Common.Interfaces;
using Beans.Models;
using Beans.Services.Interfaces;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Beans.API.Endpoints;

public static class UserEndpoints
{
    public static void ConfigureUserEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/User", Get);
        app.MapGet("/api/v1/User/Count", Count);
        app.MapGet("/api/v1/User/ExistsById/{userid}", ExistsById);
        app.MapGet("/api/v1/User/ExistsByEmail/{email}", ExistByEmail);
        app.MapGet("/api/v1/user/ExistsByIdentifier/{identifier}", ExistByIdentifier);
        app.MapGet("/api/v1/User/ById/{userid}", ById);
        app.MapGet("/api/v1/User/ByEmail/{email}", ByEmail);
        app.MapGet("/api/v1/User/ByIdentifier/{identifier}", ByIdentifier);
        app.MapGet("/api/v1/User/Names", GetNames).RequireAuthorization();
        app.MapGet("/api/v1/User/Name/{email}", GetName).RequireAuthorization();
        app.MapGet("/api/v1/User/Leaderboard", Leaderboard);
        app.MapPost("/api/v1/User", Create).RequireAuthorization();
        app.MapPost("/api/v1/User/Reset", Reset).RequireAuthorization(Constants.ADMIN_REQUIRED);
        app.MapPut("/api/v1/User/UpdateIdentifier", UpdateIdentifier).RequireAuthorization();
        app.MapPut("/api/v1/User/ChangeProfile", UpdateProfile).RequireAuthorization();
        app.MapPut("/api/v1/User/Loan/{userid}/{amount}", MakeLoan).RequireAuthorization();
        app.MapPut("/api/v1/User/Repay/{userid}/{amount}", RepayLoan).RequireAuthorization();
        app.MapPut("/api/v1/User/Toggle/{userid}", ToggleAdmin).RequireAuthorization(Constants.ADMIN_REQUIRED);
        app.MapDelete("/api/v1/User/{userid}", Delete).RequireAuthorization(Constants.ADMIN_REQUIRED);
    }

    private static async Task<IResult> Get(IUserService userService) => Results.Ok(await userService.GetAsync());

    private static IResult ForModel(UserModel? model, string key, string value)
    {
        if (model is null)
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.NotFound, "user", key, value)));
        }
        return Results.Ok(model);
    }

    private static async Task<IResult> ById(string userid, IUserService userService)
    {
        var model = await userService.ReadAsync(userid);
        return ForModel(model, "id", userid);
    }

    private static async Task<IResult> ByEmail(string email, IUserService userService)
    {
        var model = await userService.ReadForEmailAsync(email);
        return ForModel(model, "email", email);
    }

    private static async Task<IResult> ByIdentifier(string identifier, IUserService userService)
    {
        var model = await userService.ReadForIdentifierAsync(identifier);
        return ForModel(model, "identifier", identifier);
    }

    private static async Task<IResult> Count(IUserService userService) => Results.Ok(await userService.CountAsync());

    private static async Task<IResult> ExistsById(string userid, IUserService userService)
    {
        if (!string.IsNullOrWhiteSpace(userid))
        {
            var user = await userService.ReadAsync(userid);
            return Results.Ok(user is not null);
        }
        return Results.Ok(false);
    }

    private static async Task<IResult> ExistByEmail(string email, IUserService userService)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            var user = await userService.ReadAsync(email);
            return Results.Ok(user is not null);
        }
        return Results.Ok(false);
    }

    private static async Task<IResult> ExistByIdentifier(string identifier, IUserService userService)
    {
        if (!string.IsNullOrWhiteSpace(identifier))
        {
            var user = await userService.ReadForIdentifierAsync(identifier);
            return Results.Ok(user is not null);
        }
        return Results.Ok(false);
    }

    private static async Task<IResult> GetName(string email, IUserService userService)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "email")));
        }
        var user = await userService.ReadAsync(email);
        if (user is null)
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.ItemNotFound, "user", "email", email)));
        }
        return Results.Ok(NameModel.FromUserModel(user));
    }

    private static async Task<IResult> GetNames(IUserService userService)
    {
        var users = await userService.GetAsync();
        return Results.Ok(NameModel.FromUserModels(users));
    }

    private static async Task<IResult> Create([FromBody] UserModel model, IUserService userService, IUriHelper uriHelper, IOptions<AppSettings> settings)
    {
        if (model is null)
        {
            return Results.BadRequest(new ApiError(Strings.InvalidModel));
        }
        uriHelper.SetBase(settings.Value.ApiBase);
        uriHelper.SetVersion(1);
        var result = await userService.InsertAsync(model);
        if (result.Successful)
        {
            var uri = uriHelper.Create("User", "ById", model.Id);
            return Results.Created(uri.ToString(), model);
        }
        return Results.BadRequest(result);
    }

    private static async Task<IResult> Leaderboard(IUserService userService) =>
        Results.Ok((await userService.GetLeaderboardAsync()).OrderByDescending(x => x.Score));

    private static async Task<IResult> Reset(IHttpContextAccessor accessor, IUserService userService)
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
        var result = await userService.ResetUsersAsync();
        if (result.Successful)
        {
            return Results.Ok();
        }
        return Results.BadRequest(result);
    }

    private static async Task<IResult> UpdateIdentifier([FromBody] UserModel model, IUserService userService, IHttpContextAccessor accessor)
    {
        if (model is null || string.IsNullOrWhiteSpace(model.Identifier) || string.IsNullOrWhiteSpace(model.Email))
        {
            return Results.BadRequest(new ApiError(Strings.InvalidModel));
        }
        var user = await userService.ReadForEmailAsync(model.Email);
        if (user is null)
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.NotFound, "user", "email", model.Email)));
        }
        user.Identifier = model.Identifier;
        var result = await userService.UpdateAsync(user);
        if (result.Successful)
        {
            return Results.Ok();
        }
        return Results.BadRequest(result);
    }

    private static async Task<IResult> UpdateProfile([FromBody] ChangeProfileModel model, IUserService userService, IHttpContextAccessor accessor)
    {
        if (model is null || string.IsNullOrWhiteSpace(model.Identifier))
        {
            return Results.BadRequest(new ApiError(Strings.InvalidModel));
        }
        var user = await userService.ReadForIdentifierAsync(model.Identifier);
        if (user is null)
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.NotFound, "user", "identifier", model.Identifier)));
        }
        if (string.IsNullOrWhiteSpace(model.Email) && string.IsNullOrWhiteSpace(model.FirstName) && string.IsNullOrWhiteSpace(model.LastName) &&
            string.IsNullOrWhiteSpace(model.DisplayName))
        {
            return Results.Ok();
        }
        var context = accessor.HttpContext;
        if (context is null)
        {
            return Results.BadRequest(new ApiError("Cannot access HTTP Context"));
        }
        var requesteridentifier = context.GetToken()?.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;
        if (string.IsNullOrWhiteSpace(requesteridentifier))
        {
            return Results.BadRequest(new ApiError(Strings.NotAuthenticated));
        }
        var requester = await userService.ReadForIdentifierAsync(requesteridentifier);
        if (requester is null)
        {
            return Results.BadRequest(new ApiError(Strings.NotAuthenticated));
        }
        if (!string.Equals(requesteridentifier, user.Identifier, StringComparison.OrdinalIgnoreCase))
        {
            if (!requester.IsAdmin)
            {
                // only an admin can change someone else's profile
                return Results.BadRequest(new ApiError(Strings.NotAuthorized));
            }
        }
        if (!string.IsNullOrWhiteSpace(model.Email))
        {
            user.Email = model.Email;
        }
        if (!string.IsNullOrWhiteSpace(model.FirstName))
        {
            user.FirstName = model.FirstName;
        }
        if (!string.IsNullOrWhiteSpace(model.LastName))
        {
            user.LastName = model.LastName;
        }
        if (!string.IsNullOrWhiteSpace(model.DisplayName))
        {
            user.DisplayName = model.DisplayName;
        }
        var result = await userService.UpdateAsync(user);
        if (result.Successful)
        {
            return Results.Ok();
        }
        return Results.BadRequest(result);
    }

    private static async Task<IResult> MakeLoan(string userid, int amount, IUserService userService)
    {
        if (string.IsNullOrWhiteSpace(userid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Required, "user id")));
        }
        if (amount <= 0)
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "amount")));
        }
        var result = await userService.LoanAsync(userid, amount);
        if (!result.Successful)
        {
            return Results.BadRequest(result);
        }
        return Results.Ok();
    }

    private static async Task<IResult> RepayLoan(string userid, int amount, IUserService userService)
    {
        if (string.IsNullOrWhiteSpace(userid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Required, "user id")));
        }
        if (amount <= 0)
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "amount")));
        }
        var result = await userService.RepayAsync(userid, amount);
        if (!result.Successful)
        {
            return Results.BadRequest(result);
        }
        return Results.Ok();
    }

    private static async Task<IResult> ToggleAdmin(string userid, IUserService userService)
    {
        if (string.IsNullOrWhiteSpace(userid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "user id")));
        }
        var result = await userService.ToggleAdminAsync(userid);
        if (result.Successful)
        {
            return Results.Ok();
        }
        return Results.BadRequest(result);
    }

    private static async Task<IResult> Delete(string email, IUserService userService, IHttpContextAccessor accessor)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "email")));
        }
        var token = accessor.HttpContext?.GetToken();
        if (token is null)
        {
            return Results.BadRequest(new ApiError(Strings.NotAuthenticated));
        }
        var identifier = token.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return Results.BadRequest(new ApiError(Strings.NotAuthenticated));
        }
        var thisuser = await userService.ReadForIdentifierAsync(identifier);
        if (thisuser is null)
        {
            return Results.BadRequest(new ApiError(Strings.NotAuthenticated));
        }
        if (!thisuser.IsAdmin)
        {
            return Results.BadRequest(new ApiError(Strings.NotAuthorized));
        }
        var user = await userService.ReadAsync(email);
        if (user is null)
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.ItemNotFound, "user", "email", email)));
        }
        if (user.IsAdmin)
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.AdminUsers, "deleted")));
        }
        if (!user.CanDelete)
        {
            return Results.BadRequest(new ApiError(Strings.CantDelete));
        }
        var result = await userService.DeleteAsync(user);
        if (result.Successful)
        {
            return Results.Ok();
        }
        return Results.BadRequest(new ApiError(result.ErrorMessage()));
    }
}
