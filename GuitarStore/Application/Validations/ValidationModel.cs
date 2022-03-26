using System.Net;

namespace Application.Validations;

public class ValidationModel
{
    public HttpStatusCode HttpStatusCode { get; set; }
    public string? Message { get; set; }
    public string? Details { get; set; }
    public string? BusinessErrorCode { get; set; }
}
