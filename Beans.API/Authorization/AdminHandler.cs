using Beans.API.Infrastructure;
using Beans.Services.Interfaces;

using Microsoft.AspNetCore.Authorization;

namespace Beans.API.Authorization;

public class AdminHandler : AuthorizationHandler<AdminRequirement>
{
    private readonly IUserService _userService;
    private readonly IHttpContextAccessor _accessor;

    public AdminHandler(IUserService userService, IHttpContextAccessor accessor)
    {
        _userService = userService;
        _accessor = accessor;
    }

    protected async override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
    {
        if (context is null || requirement is null)
        {
            throw new InvalidOperationException("Context and requirement are required");
        }
        if (!(context?.User?.Identity?.IsAuthenticated ?? false))
        {
            context!.Fail(new(this, "User is not authenticated"));
            return;
        }
        var token = _accessor.HttpContext?.GetToken();
        if (token is null)
        {
            context!.Fail(new(this, "No Token found"));
            return;
        }
        var identifier = token.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;
        if (string.IsNullOrWhiteSpace(identifier))
        {
            context!.Fail(new(this, "No Identifier found"));
            return;
        }
        var user = await _userService.ReadForIdentifierAsync(identifier);
        if (user is null)
        {
            context!.Fail(new(this, "User not found"));
            return;
        }
        if (user.IsAdmin != requirement.MustBeAdmin)
        {
            if (requirement.MustBeAdmin)
            {
                context!.Fail(new(this, "User is not an admin and must be"));
            }
            else
            {
                context!.Fail(new(this, "User is an admin and must not be"));
            }
            return;
        }
        context!.Succeed(requirement);
    }
}
