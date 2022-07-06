using Microsoft.AspNetCore.Authorization;

namespace Beans.API.Authorization;

public class AdminRequirement : IAuthorizationRequirement
{
    public bool MustBeAdmin { get; }

    public AdminRequirement(bool mustBeAdmin = true) => MustBeAdmin = mustBeAdmin;
}
