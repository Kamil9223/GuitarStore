using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace GuitarStore.Api.Client;
public static class ApiExceptionExtension
{
    public static ProblemDetails ToFailedResponse(this ApiException exception)
    {
        return JsonConvert.DeserializeObject<ProblemDetails>(exception.Response)!;
    }
}
