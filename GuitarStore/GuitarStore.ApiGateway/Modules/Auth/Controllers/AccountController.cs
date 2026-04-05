using Auth.Core.Services;
using GuitarStore.ApiGateway.Modules.Auth.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GuitarStore.ApiGateway.Modules.Auth.Controllers;

[AllowAnonymous]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class AccountController(IAuthService authService) : Controller
{
    [HttpGet("~/auth/login")]
    public IActionResult Login([FromQuery] string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost("~/auth/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await authService.LoginAsync(new AuthLoginRequest(
            model.EmailOrUserName,
            model.Password,
            model.RememberMe));

        if (result.Succeeded)
        {
            return RedirectToLocal(model.ReturnUrl);
        }

        switch (result.Status)
        {
            case AuthLoginStatus.LockedOut:
                ModelState.AddModelError(string.Empty, "This account is locked.");
                break;
            case AuthLoginStatus.NotAllowed:
                ModelState.AddModelError(string.Empty, "This account is not allowed to sign in.");
                break;
            default:
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                break;
        }

        return View(model);
    }

    [HttpGet("~/auth/register")]
    public IActionResult Register([FromQuery] string? returnUrl = null)
    {
        return View(new RegisterViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost("~/auth/register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await authService.RegisterAsync(new AuthRegisterRequest(
            model.Name,
            model.LastName,
            model.Email,
            model.Password), ct);

        if (result.Succeeded)
        {
            return RedirectToLocal(model.ReturnUrl);
        }

        if (result.Status == AuthRegisterStatus.DuplicateEmail)
        {
            ModelState.AddModelError(nameof(RegisterViewModel.Email), "A user with this email already exists.");
            return View(model);
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        return View(model);
    }

    [HttpPost("~/auth/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout([FromForm] string? returnUrl = null)
    {
        await authService.LogoutAsync();
        return RedirectToLocal(returnUrl);
    }

    [HttpGet("~/auth/forbidden")]
    public IActionResult Forbidden()
    {
        return View();
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return Redirect(Url.Content("~/"));
    }
}
