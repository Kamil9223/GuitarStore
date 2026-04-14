using Auth.Core.Entities;
using GuitarStore.ApiGateway.Modules.Auth.Services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Server.AspNetCore;

namespace GuitarStore.ApiGateway.Modules.Auth.Controllers;

[AllowAnonymous]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class OpenIddictController(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    IOidcClaimsPrincipalFactory oidcClaimsPrincipalFactory) : Controller
{
    [AcceptVerbs("GET", "POST")]
    [Route("~/connect/authorize")]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("OpenIddict authorize request is not available.");

        var authenticationResult = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (!authenticationResult.Succeeded)
        {
            return Challenge(
                new AuthenticationProperties { RedirectUri = BuildReturnUrl() },
                IdentityConstants.ApplicationScheme);
        }

        var user = await userManager.GetUserAsync(authenticationResult.Principal!);
        if (user is null)
        {
            await signInManager.SignOutAsync();

            return Challenge(
                new AuthenticationProperties { RedirectUri = BuildReturnUrl() },
                IdentityConstants.ApplicationScheme);
        }

        if (!await signInManager.CanSignInAsync(user))
        {
            await signInManager.SignOutAsync();
            return RedirectToAction(nameof(AccountController.Forbidden), "Account");
        }

        if (user.MustChangePassword)
        {
            return RedirectToAction(
                nameof(AccountController.ChangePasswordRequired),
                "Account",
                new { returnUrl = BuildReturnUrl() });
        }

        var principal = await oidcClaimsPrincipalFactory.CreateAsync(user, request);
        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [AcceptVerbs("GET", "POST")]
    [Route("~/connect/logout")]
    public async Task<IActionResult> Logout()
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        await signInManager.SignOutAsync();

        if (request is null)
        {
            return Redirect(Url.Content("~/"));
        }

        var properties = new AuthenticationProperties();
        if (!string.IsNullOrWhiteSpace(request.PostLogoutRedirectUri))
        {
            properties.RedirectUri = request.PostLogoutRedirectUri;
        }

        return SignOut(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private string BuildReturnUrl()
    {
        var parameters = Request.HasFormContentType
            ? Request.Form.SelectMany(
                static pair => pair.Value,
                static (pair, value) => new KeyValuePair<string, string?>(pair.Key, value))
            : Request.Query.SelectMany(
                static pair => pair.Value,
                static (pair, value) => new KeyValuePair<string, string?>(pair.Key, value));

        return Request.PathBase + Request.Path + QueryString.Create(parameters);
    }
}
