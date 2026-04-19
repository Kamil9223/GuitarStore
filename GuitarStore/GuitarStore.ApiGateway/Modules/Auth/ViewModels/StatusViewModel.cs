namespace GuitarStore.ApiGateway.Modules.Auth.ViewModels;

public sealed class StatusViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Heading { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? PrimaryActionText { get; set; }
    public string? PrimaryActionUrl { get; set; }
}
