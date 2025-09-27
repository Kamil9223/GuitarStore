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
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "GuitarStore API V1");
            });
        }

        //app.UseHttpsRedirection();

        app.UseAuthorization();

        app.UseMiddleware<ExceptionsMiddleware>();

        app.MapControllers();

        app.Run();
    }
}
