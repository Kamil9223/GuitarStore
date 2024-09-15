using Domain.StronglyTypedIds;
using Domain.StronglyTypedIds.Helpers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GuitarStore.ApiGateway.Helpers.StronglyTypedIdsConfig;

public class StronglyTypedIdSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (typeof(IStronglyTypedId).IsAssignableFrom(context.Type))
        {
            schema.Type = "string";
            schema.Format = "uuid";
            schema.Properties.Clear();
        }
    }
}
