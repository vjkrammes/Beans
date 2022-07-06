using Beans.API.Infrastructure;
using Beans.Common;
using Beans.Models;
using Beans.Services.Interfaces;

using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Options;

namespace Beans.API.Endpoints;

public static class OfferEndpoints
{
    public static void ConfigureOfferEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/Offer", Get);
        app.MapGet("/api/v1/Offer/ById/{offerid}", ById);
        app.MapGet("/api/v1/Offer/OldestFirst", OldestFirst).RequireAuthorization();
        app.MapGet("/api/v1/Offer/MyOffers/{userid}", MyOffers).RequireAuthorization();
        app.MapGet("/api/v1/Offer/BySeller/{sellerid}/{beanid?}", OffersBySeller).RequireAuthorization();
        app.MapGet("/api/v1/Offer/ByBean/{beanid}", OffersByBean).RequireAuthorization();
        app.MapGet("/api/v1/Offer/Others/{userid}", OtherOffers).RequireAuthorization();
        app.MapPost("/api/v1/Offer", CreateNewOffer).RequireAuthorization();
        app.MapPost("/api/v1/Offer/{userid}/{beanid}/{holdingid}/{quantity}/{price}/{buysell}", Create).RequireAuthorization();
        app.MapPost("/api/v1/Offer/Buy/{buyerid}/{quantity}/{offerid}", BuyFromOffer).RequireAuthorization();
        app.MapPost("/api/v1/Offer/Sell", SellToOffer).RequireAuthorization();
        app.MapPut("/api/v1/Offer", Update).RequireAuthorization();
        app.MapDelete("/api/v1/Offer/{offerid}", Delete).RequireAuthorization();
    }

    private static async Task<IResult> Get(IOfferService offerService) => Results.Ok(await offerService.GetAsync());

    private static async Task<IResult> ById(string offerid, IOfferService offerService)
    {
        var model = await offerService.ReadAsync(offerid);
        if (model is null)
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.NotFound, "offer", "id", offerid)));
        }
        return Results.Ok(model);
    }

    public static IResult OldestFirst(IOptions<AppSettings> settings) => Results.Ok(settings.Value.OldestFirst);

    public static async Task<IResult> MyOffers(string userid, IOfferService offerService)
    {
        if (string.IsNullOrWhiteSpace(userid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "user id")));
        }
        var offers = await offerService.GetForUserAsync(userid);
        return Results.Ok(offers);
    }

    public static async Task<IResult> OffersBySeller(string sellerid, string? beanid, IOfferService offerService)
    {
        if (string.IsNullOrWhiteSpace(sellerid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "seller id")));
        }
        var offers = await offerService.GetOffersAsync(sellerid, beanid, false);
        return Results.Ok(offers);
    }

    public static async Task<IResult> OffersByBean(string beanid, IOfferService offerService)
    {
        if (string.IsNullOrWhiteSpace(beanid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "bean id")));
        }
        var offers = await offerService.GetForBeanAsync(beanid);
        return Results.Ok(offers);
    }

    public static async Task<IResult> OtherOffers(string userid, IOfferService offerService)
    {
        if (string.IsNullOrWhiteSpace(userid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "user id")));
        }
        var offers = await offerService.GetOtherOffersAsync(userid);
        return Results.Ok(offers);
    }

    public static async Task<IResult> CreateNewOffer([FromBody] OfferModel model, IOfferService offerService)
    {
        if (model is null)
        {
            return Results.BadRequest(new ApiError(Strings.InvalidModel));
        }
        return await Create(model.UserId, model.BeanId, model.HoldingId, model.Quantity, model.Price,
            model.Buy ? "true" : "false", offerService);
    }

    public static async Task<IResult> Update([FromBody] OfferModel model, IOfferService offerService)
    {
        if (model is null)
        {
            return Results.BadRequest(new ApiError(Strings.InvalidModel)); ;
        }
        var response = await offerService.UpdateAsync(model);
        if (response.Successful)
        {
            return Results.Ok();
        }
        return Results.BadRequest(response);
    }

    public static async Task<IResult> Delete(string offerid, IOfferService offerService, IUserService userService, IHttpContextAccessor contextAccessor)
    {
        if (string.IsNullOrWhiteSpace(offerid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "offer id")));
        }
        var offer = await offerService.ReadAsync(offerid);
        if (offer is null)
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.ItemNotFound, "offer", "id", offerid)));
        }
        var request = contextAccessor.HttpContext?.Request;
        if (request is null)
        {
            return Results.BadRequest(new ApiError(Strings.ContextNotFound));
        }
        var token = request.GetToken();
        if (token is null)
        {
            return Results.BadRequest(new ApiError(Strings.NotAuthenticated));
        }
        var identifier = token.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return Results.BadRequest(new ApiError(Strings.NotAuthenticated));
        }
        var user = await userService.ReadForIdentifierAsync(identifier);
        if (user is null)
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.ItemNotFound, "user", "identifier", identifier)));
        }
        if (user.Id != offer.UserId)
        {
            return Results.BadRequest(new ApiError(Strings.NotAuthorized));
        }
        var result = await offerService.DeleteAsync(offer);
        if (result.Successful)
        {
            return Results.Ok();
        }
        return Results.BadRequest(new ApiError(result.ErrorMessage()));
    }

    public static async Task<IResult> BuyFromOffer(string buyerid, long quantity, string offerid, IOfferService offerService, IOptions<AppSettings> settings)
    {
        if (string.IsNullOrWhiteSpace(buyerid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "buyer id")));
        }
        if (quantity <= 0)
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "quantity")));
        }
        var oldestFirst = settings.Value.OldestFirst;
        var result = await offerService.BuyFromOfferAsync(buyerid, quantity, offerid, oldestFirst);
        if (result.Successful)
        {
            return Results.Ok();
        }
        return Results.BadRequest(new ApiError(result.ErrorMessage()));
    }

    public static async Task<IResult> SellToOffer([FromBody] SellToOfferModel model, IOfferService offerService)
    {
        var response = await offerService.SellToOfferAsync(model.OfferId, model.SellerId, model.Items);
        if (response.Successful)
        {
            return Results.Ok();
        }
        return Results.BadRequest(response.Message);
    }

    public static async Task<IResult> Create(string userid, string beanid, string holdingid, long quantity, decimal price, string buysell, IOfferService offerService)
    {
        if (string.IsNullOrWhiteSpace(userid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "user id")));
        }
        if (string.IsNullOrWhiteSpace(beanid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "bean id")));
        }
        if (string.IsNullOrWhiteSpace(holdingid))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.Invalid, "holding id")));
        }
        if (quantity <= 0 || price <= 0M)
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.GTZero, "Quantity and price")));
        }
        if (!bool.TryParse(buysell, out var buy))
        {
            return Results.BadRequest(new ApiError(string.Format(Strings.BuySell, buysell)));
        }
        var result = await offerService.CreateAsync(userid, beanid, holdingid, quantity, price, buy);
        if (!result.Successful)
        {
            return Results.BadRequest(new ApiError(result.ErrorMessage()));
        }
        return Results.Ok();
    }
}
