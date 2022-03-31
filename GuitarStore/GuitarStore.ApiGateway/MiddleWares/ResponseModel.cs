using System.Net;

namespace GuitarStore.ApiGateway.MiddleWares;

internal class ResponseModel
{
    public HttpStatusCode HttpStatusCode { get; init; }
    public string? Message { get; init; }
    public string? BusinessErrorCode { get; init; }

    public ResponseModel(HttpStatusCode httpStatusCode, string? message, string? businessErrorCode)
    {
        HttpStatusCode = httpStatusCode;
        Message = message;
        BusinessErrorCode = businessErrorCode;
    }
}
