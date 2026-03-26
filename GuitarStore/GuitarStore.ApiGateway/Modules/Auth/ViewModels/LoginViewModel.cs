using System.ComponentModel.DataAnnotations;

namespace GuitarStore.ApiGateway.Modules.Auth.ViewModels;

public sealed class LoginViewModel
{
    [Required]
    [Display(Name = "Email or user name")]
    public string EmailOrUserName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
