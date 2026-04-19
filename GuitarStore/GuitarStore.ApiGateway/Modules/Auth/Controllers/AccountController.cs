using Auth.Core.Authorization;
using Auth.Core.Services;
using GuitarStore.ApiGateway.Modules.Auth.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GuitarStore.ApiGateway.Modules.Auth.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public sealed class AccountController(IAuthService authService) : Controller
{
    [AllowAnonymous]
    [HttpGet("~/auth/login")]
    public IActionResult Login([FromQuery] string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
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

        if (result.Status == AuthLoginStatus.RequiresPasswordChange)
        {
            return RedirectToAction(nameof(ChangePasswordRequired), new { returnUrl = model.ReturnUrl });
        }

        switch (result.Status)
        {
            case AuthLoginStatus.LockedOut:
                ModelState.AddModelError(string.Empty, "This account is locked.");
                break;
            case AuthLoginStatus.RequiresEmailConfirmation:
                ModelState.AddModelError(string.Empty, "Please confirm your email before signing in.");
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

    [AllowAnonymous]
    [HttpGet("~/auth/register")]
    public IActionResult Register([FromQuery] string? returnUrl = null)
    {
        return View(new RegisterViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
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
            if (result.RequiresEmailConfirmation)
            {
                return RedirectToAction(nameof(RegisterConfirmation), new { email = model.Email });
            }

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

    [AllowAnonymous]
    [HttpGet("~/auth/register-confirmation")]
    public IActionResult RegisterConfirmation([FromQuery] string? email = null)
    {
        return View("Status", new StatusViewModel
        {
            Title = "Confirm your email",
            Heading = "Check your inbox",
            Message = string.IsNullOrWhiteSpace(email)
                ? "We sent a confirmation link to your email address. Confirm it before signing in."
                : $"We sent a confirmation link to {email}. Confirm it before signing in.",
            PrimaryActionText = "Back to sign in",
            PrimaryActionUrl = Url.Action(nameof(Login), "Account")
        });
    }

    [AllowAnonymous]
    [HttpGet("~/auth/confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string? userId = null, [FromQuery] string? token = null)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
        {
            return View("Status", BuildFailureStatus(
                "Confirm email",
                "This confirmation link is invalid or incomplete."));
        }

        var result = await authService.ConfirmEmailAsync(userId, token);
        if (result.Status == AuthConfirmEmailStatus.InvalidTokenOrUser)
        {
            return View("Status", BuildFailureStatus(
                "Confirm email",
                "This confirmation link is invalid or has expired."));
        }

        return View("Status", new StatusViewModel
        {
            Title = "Email confirmed",
            Heading = result.Status == AuthConfirmEmailStatus.AlreadyConfirmed
                ? "Email already confirmed"
                : "Email confirmed",
            Message = result.Status == AuthConfirmEmailStatus.AlreadyConfirmed
                ? "This email address is already confirmed. You can sign in."
                : "Your email address is confirmed. You can now sign in.",
            PrimaryActionText = "Go to sign in",
            PrimaryActionUrl = Url.Action(nameof(Login), "Account")
        });
    }

    [AllowAnonymous]
    [HttpGet("~/auth/forgot-password")]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    [AllowAnonymous]
    [HttpPost("~/auth/forgot-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await authService.ForgotPasswordAsync(new AuthForgotPasswordRequest(model.Email), ct);
        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [AllowAnonymous]
    [HttpGet("~/auth/forgot-password-confirmation")]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View("Status", new StatusViewModel
        {
            Title = "Password reset",
            Heading = "If the account exists, the email is on its way",
            Message = "If the address belongs to a confirmed account, you will receive password reset instructions shortly.",
            PrimaryActionText = "Back to sign in",
            PrimaryActionUrl = Url.Action(nameof(Login), "Account")
        });
    }

    [AllowAnonymous]
    [HttpGet("~/auth/reset-password")]
    public IActionResult ResetPassword([FromQuery] string? userId = null, [FromQuery] string? token = null)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
        {
            return View("Status", BuildFailureStatus(
                "Reset password",
                "This password reset link is invalid or incomplete."));
        }

        return View(new ResetPasswordViewModel
        {
            UserId = userId,
            Token = token
        });
    }

    [AllowAnonymous]
    [HttpPost("~/auth/reset-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await authService.ResetPasswordAsync(new AuthResetPasswordRequest(
            model.UserId,
            model.Token,
            model.NewPassword));

        if (result.Succeeded)
        {
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        if (result.Status == AuthResetPasswordStatus.InvalidTokenOrUser)
        {
            ModelState.AddModelError(string.Empty, "This password reset link is invalid or has expired.");
            return View(model);
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        return View(model);
    }

    [AllowAnonymous]
    [HttpGet("~/auth/reset-password-confirmation")]
    public IActionResult ResetPasswordConfirmation()
    {
        return View("Status", new StatusViewModel
        {
            Title = "Password updated",
            Heading = "Password updated",
            Message = "Your password has been changed. Sign in with the new password to continue.",
            PrimaryActionText = "Go to sign in",
            PrimaryActionUrl = Url.Action(nameof(Login), "Account")
        });
    }

    [Authorize(AuthenticationSchemes = AuthAuthenticationSchemes.IdentityApplication)]
    [HttpGet("~/auth/change-password-required")]
    public async Task<IActionResult> ChangePasswordRequired([FromQuery] string? returnUrl = null)
    {
        if (!await authService.RequiresPasswordChangeAsync(User))
        {
            return RedirectToLocal(returnUrl);
        }

        return View(new ForcedPasswordChangeViewModel
        {
            ReturnUrl = returnUrl
        });
    }

    [Authorize(AuthenticationSchemes = AuthAuthenticationSchemes.IdentityApplication)]
    [HttpPost("~/auth/change-password-required")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePasswordRequired(ForcedPasswordChangeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await authService.ChangePasswordAsync(User, new AuthChangePasswordRequest(
            model.CurrentPassword,
            model.NewPassword));

        if (result.Succeeded || result.Status == AuthChangePasswordStatus.PasswordChangeNotRequired)
        {
            return RedirectToLocal(model.ReturnUrl);
        }

        if (result.Status == AuthChangePasswordStatus.CurrentUserNotFound)
        {
            return RedirectToAction(nameof(Login), new { returnUrl = model.ReturnUrl });
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        return View(model);
    }

    [Authorize(AuthenticationSchemes = AuthAuthenticationSchemes.IdentityApplication)]
    [HttpPost("~/auth/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout([FromForm] string? returnUrl = null)
    {
        await authService.LogoutAsync();
        return RedirectToLocal(returnUrl);
    }

    [AllowAnonymous]
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

    private StatusViewModel BuildFailureStatus(string title, string message)
    {
        return new StatusViewModel
        {
            Title = title,
            Heading = title,
            Message = message,
            PrimaryActionText = "Back to sign in",
            PrimaryActionUrl = Url.Action(nameof(Login), "Account")
        };
    }
}
