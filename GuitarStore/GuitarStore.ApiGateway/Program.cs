using GuitarStore.ApiGateway.Configuration;
using GuitarStore.ApiGateway.Helpers.StronglyTypedIdsConfig;
using GuitarStore.ApiGateway.MiddleWares;
using Microsoft.OpenApi.Models;

public class Program
{
    public static void Main(string[] args)
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

        builder.Services.AddControllers();
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

        app.Run();
    }
}
