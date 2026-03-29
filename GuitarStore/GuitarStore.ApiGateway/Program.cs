using GuitarStore.ApiGateway.Configuration;
using GuitarStore.ApiGateway.Helpers.StronglyTypedIdsConfig;
using GuitarStore.ApiGateway.MiddleWares;
using GuitarStore.ApiGateway.Modules.Auth.Services;
using Microsoft.OpenApi.Models;
using System.Diagnostics;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        if (builder.Environment.IsDevelopment())
        {
            DotNetEnv.Env.Load(options: new DotNetEnv.LoadOptions());
        }

        builder.Configuration
            .AddJsonFile("appsettings.Auth.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.Catalog.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.Customers.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.Delivery.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.Orders.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.Payments.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.Warehouse.json", optional: true, reloadOnChange: true);

        builder.Services.InitializeModules(builder.Configuration);
        builder.Services.AddScoped<IOidcClaimsPrincipalFactory, OidcClaimsPrincipalFactory>();

        builder.Services.AddControllersWithViews();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            options.SchemaFilter<StronglyTypedIdSchemaFilter>();
            options.MapType<decimal>(() => new OpenApiSchema { Type = "number", Format = "decimal" });
            options.MapType<decimal?>(() => new OpenApiSchema { Type = "number", Format = "decimal", Nullable = true });
        });

        var app = builder.Build();
        if (!IsOpenApiDocumentGeneration())
        {
            await app.InitializeModulesAsync();
        }

        app.MapGet("/test", () => Results.Ok("Hello World!"));

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "GuitarStore API V1");
            });
        }

        app.UseRouting();
        app.UseAuthentication();
        //app.UseHttpsRedirection();
        app.UseAuthorization();

        app.UseMiddleware<ExceptionsMiddleware>();

        app.MapControllers();

        await app.RunAsync();
    }

    private static bool IsOpenApiDocumentGeneration()
    {
        var commandLine = Environment.CommandLine;

        return commandLine.Contains("dotnet-getdocument", StringComparison.OrdinalIgnoreCase)
            || commandLine.Contains("GetDocument.Insider", StringComparison.OrdinalIgnoreCase)
            || Process.GetCurrentProcess().ProcessName.Contains("GetDocument", StringComparison.OrdinalIgnoreCase);
    }
}
