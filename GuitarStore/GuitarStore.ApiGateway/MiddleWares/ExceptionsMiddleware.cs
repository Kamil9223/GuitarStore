using Application.Exceptions;
using Newtonsoft.Json;
using System.Net;

namespace GuitarStore.ApiGateway.MiddleWares;

public class ExceptionsMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleException(ex, context);
        }
    }

    private async Task HandleException(Exception ex, HttpContext context)
    {
        var responseModel = ex switch
        {
            ValidationException => new ResponseModel(HttpStatusCode.BadRequest, ex.Message, null),
            _ => new ResponseModel(HttpStatusCode.InternalServerError, ex.Message, null)
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)responseModel.HttpStatusCode;

        await context.Response.WriteAsync(JsonConvert.SerializeObject(responseModel));
    }
}
