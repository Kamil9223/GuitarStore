using Auth.Core.Authorization;
using Auth.Core.Entities;
using Common.StronglyTypedIds.StronglyTypedIds;
using GuitarStore.ApiGateway.Modules.Auth.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GuitarStore.ApiGateway.Modules.Auth.Controllers;

[AllowAnonymous]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class AccountController(
    UserManager<User> userManager,
    SignInManager<User> signInManager) : Controller
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

        var user = await FindUserAsync(model.EmailOrUserName);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        var result = await signInManager.PasswordSignInAsync(
            user.UserName!,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            return RedirectToLocal(model.ReturnUrl);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "This account is locked.");
        }
        else if (result.IsNotAllowed)
        {
            ModelState.AddModelError(string.Empty, "This account is not allowed to sign in.");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
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
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (await userManager.FindByEmailAsync(model.Email) is not null)
        {
            ModelState.AddModelError(nameof(RegisterViewModel.Email), "A user with this email already exists.");
            return View(model);
        }

        var user = new User
        {
            Id = AuthId.New(),
            UserName = model.Email,
            Email = model.Email
        };

        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            return View(model);
        }

        var addToRoleResult = await userManager.AddToRoleAsync(user, AuthRoles.User);
        if (!addToRoleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            AddIdentityErrors(addToRoleResult);
            return View(model);
        }

        await signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToLocal(model.ReturnUrl);
    }

    [HttpPost("~/auth/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout([FromForm] string? returnUrl = null)
    {
        await signInManager.SignOutAsync();
        return RedirectToLocal(returnUrl);
    }

    [HttpGet("~/auth/forbidden")]
    public IActionResult Forbidden()
    {
        return View();
    }

    private async Task<User?> FindUserAsync(string emailOrUserName)
    {
        return await userManager.FindByEmailAsync(emailOrUserName)
            ?? await userManager.FindByNameAsync(emailOrUserName);
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return Redirect(Url.Content("~/"));
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
