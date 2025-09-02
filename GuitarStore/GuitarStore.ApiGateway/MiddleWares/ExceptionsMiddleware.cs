using Common.Errors.Exceptions;
using Microsoft.AspNetCore.Mvc;
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
        var problemDetails = ex switch
        {
            ValidationException => new ProblemDetails
            {
                Title = ((ValidationException)ex).Title,
                Status = (int)HttpStatusCode.BadRequest,
                Detail = ex.Message,
                Instance = context.Request.Path
            },
            NotFoundException => new ProblemDetails
            {
                Title = ((NotFoundException)ex).Title,
                Status = (int)HttpStatusCode.NotFound,
                Detail = ex.Message,
                Instance = context.Request.Path
            },
            DomainException => new ProblemDetails
            {
                Title = ((DomainException)ex).Title,
                Status = (int)HttpStatusCode.Conflict,
                Detail = ((DomainException)ex).ErrorCode,
                Instance = context.Request.Path
            },
            _ => new ProblemDetails
            {
                Title = "Unknow_Error",
                Status = (int)HttpStatusCode.InternalServerError,
                Detail = ex.Message,
                Instance = context.Request.Path
            },
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = problemDetails.Status!.Value;

        await context.Response.WriteAsync(JsonConvert.SerializeObject(problemDetails));
    }
}
