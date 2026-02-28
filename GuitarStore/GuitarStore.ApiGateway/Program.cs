using Auth.Core;
using GuitarStore.ApiGateway.Configuration;
using GuitarStore.ApiGateway.Helpers.StronglyTypedIdsConfig;
using GuitarStore.ApiGateway.MiddleWares;
using Microsoft.OpenApi.Models;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.InitializeModules(builder.Configuration);

        builder.Services.AddControllers();
        builder.Services.AddHttpContextAccessor();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            options.SchemaFilter<StronglyTypedIdSchemaFilter>();
            options.MapType<decimal>(() => new OpenApiSchema { Type = "number", Format = "decimal" });
            options.MapType<decimal?>(() => new OpenApiSchema { Type = "number", Format = "decimal", Nullable = true });
        });

        builder.Services.AddIdentityServer()
            .AddInMemoryIdentityResources(DuendeConfig.IdentityResources)
            .AddInMemoryApiScopes(DuendeConfig.ApiScopes)
            .AddInMemoryApiResources(DuendeConfig.ApiResources)
            .AddInMemoryClients(DuendeConfig.Clients)
            .AddDeveloperSigningCredential(); // DEV only

        var app = builder.Build();

        app.MapGet("/test", () => Results.Ok("Hello World!"));

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "GuitarStore API V1");
            });
        }

        app.UseRouting();
        app.UseIdentityServer(); // publikuje /.well-known/openid-configuration i ca�� reszt�

        //app.UseHttpsRedirection();

        app.UseAuthorization();

        app.UseMiddleware<ExceptionsMiddleware>();

        app.MapControllers();

        app.Run();
    }
}
