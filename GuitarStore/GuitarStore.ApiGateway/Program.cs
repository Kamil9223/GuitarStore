using System.Diagnostics;
using GuitarStore.ApiGateway.Configuration;
using GuitarStore.ApiGateway.Helpers.StronglyTypedIdsConfig;
using GuitarStore.ApiGateway.MiddleWares;
using GuitarStore.ApiGateway.Modules.Auth.Services;
using Microsoft.OpenApi.Models;

namespace GuitarStore.ApiGateway;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var env = builder.Environment;

        // DotNetEnv must be loaded before rebuilding the pipeline
        // so that variables from .env are visible to AddEnvironmentVariables()
        if (env.IsDevelopment())
        {
            DotNetEnv.Env.Load(options: new DotNetEnv.LoadOptions());
        }

        // Rebuild the configuration pipeline with correct priority order:
        // JSON files (lower) → User Secrets → environment variables → CLI (higher)
        builder.Configuration.Sources.Clear();

        // dotnet-getdocument (OpenAPI generation) runs the app without ASPNETCORE_ENVIRONMENT set,
        // so we force 'Development' as the config environment —
        // otherwise modules would fail startup validation (missing required values like Auth:Issuer).
        var configEnvName = IsOpenApiDocumentGeneration() ? "Development" : env.EnvironmentName;

        // Layer 1: shared infrastructure
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{configEnvName}.json", optional: true, reloadOnChange: true);

        // Layer 2: module configs (base + per-environment override)
        string[] modules = ["Auth", "Catalog", "Customers", "Delivery", "Orders", "Payments", "Warehouse"];
        foreach (var module in modules)
        {
            builder.Configuration
                .AddJsonFile($"appsettings.{module}.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{module}.{configEnvName}.json", optional: true, reloadOnChange: true);
        }

        // Layer 3: overrides (highest priority — win over JSON files)
        if (env.IsDevelopment())
        {
            builder.Configuration.AddUserSecrets<Program>(optional: true);
        }
        builder.Configuration
            .AddEnvironmentVariables()
            .AddCommandLine(args);

        builder.Services.InitializeModules(builder.Configuration, IsOpenApiDocumentGeneration());
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