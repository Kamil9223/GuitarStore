using Application.CQRS.Command;
using Application.CQRS.Query;
using Auth.Core.Authorization;
using Auth.Core.Commands;
using Auth.Core.Queries;
using GuitarStore.ApiGateway.Modules.Auth.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GuitarStore.ApiGateway.Modules.Auth.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public sealed class AccountController : Controller
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
    public async Task<IActionResult> Login(
        LoginViewModel model,
        [FromServices] ICommandHandler<AuthLoginResult, LoginCommand> handler,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await handler.Handle(new LoginCommand(
            model.EmailOrUserName,
            model.Password,
            model.RememberMe),
            ct);

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
    public async Task<IActionResult> Register(
        RegisterViewModel model,
        [FromServices] ICommandHandler<AuthRegisterResult, RegisterUserCommand> handler,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await handler.Handle(new RegisterUserCommand(
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
    public async Task<IActionResult> ConfirmEmail(
        [FromQuery] string? userId,
        [FromQuery] string? token,
        [FromServices] ICommandHandler<AuthConfirmEmailResult, ConfirmEmailCommand> handler,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
        {
            return View("Status", BuildFailureStatus(
                "Confirm email",
                "This confirmation link is invalid or incomplete."));
        }

        var result = await handler.Handle(new ConfirmEmailCommand(userId, token), ct);
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
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordViewModel model,
        [FromServices] ICommandHandler<RequestPasswordResetCommand> handler,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await handler.Handle(new RequestPasswordResetCommand(model.Email), ct);
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
    public async Task<IActionResult> ResetPassword(
        ResetPasswordViewModel model,
        [FromServices] ICommandHandler<AuthResetPasswordResult, ResetPasswordCommand> handler,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await handler.Handle(new ResetPasswordCommand(
            model.UserId,
            model.Token,
            model.NewPassword),
            ct);

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
    public async Task<IActionResult> ChangePasswordRequired(
        [FromQuery] string? returnUrl,
        [FromServices] IQueryHandler<RequiresPasswordChangeQuery, RequiresPasswordChangeQueryResult> handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new RequiresPasswordChangeQuery(User), ct);
        if (!result.IsRequired)
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
    public async Task<IActionResult> ChangePasswordRequired(
        ForcedPasswordChangeViewModel model,
        [FromServices] ICommandHandler<AuthChangePasswordResult, ChangePasswordCommand> handler,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await handler.Handle(new ChangePasswordCommand(
            User,
            model.CurrentPassword,
            model.NewPassword),
            ct);

        if (result.Succeeded)
        {
            return RedirectToLocal(model.ReturnUrl);
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
    public async Task<IActionResult> Logout(
        [FromForm] string? returnUrl,
        [FromServices] ICommandHandler<LogoutCommand> handler,
        CancellationToken ct)
    {
        await handler.Handle(new LogoutCommand(), ct);
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
