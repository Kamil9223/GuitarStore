using Auth.Core.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GuitarStore.ApiGateway.Modules.Auth.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public sealed class AuthorizationProbeController : ControllerBase
{
    [Authorize(AuthenticationSchemes = AuthAuthenticationSchemes.IdentityApplication, Policy = AuthPolicies.CatalogManage)]
    [HttpGet("~/authz/probes/catalog-manage")]
    public IActionResult CatalogManage()
    {
        return Ok();
    }

    [Authorize(AuthenticationSchemes = AuthAuthenticationSchemes.IdentityApplication, Policy = AuthPolicies.OrdersCancelAny)]
    [HttpGet("~/authz/probes/orders-cancel-any")]
    public IActionResult OrdersCancelAny()
    {
        return Ok();
    }
}
