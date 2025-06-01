using Application.CQRS;
using Application.RabbitMq.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Payments.Core.Services;
using Payments.Shared.Services;
using Stripe;
using System.Net.Http.Headers;
using System.Reflection;

namespace Payments.Core;
public static class PaymentsModuleInitializator
{
    public static IServiceCollection AddPaymentsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();
        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IIntegrationEventHandler<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        services.AddHttpClient("StripeClient")
        .ConfigureHttpClient((provider, client) =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var stripeUrl = config["Stripe:Url"];
            client.BaseAddress = new Uri(stripeUrl!);
        });
        services.AddScoped<StripeClient>(provider =>
        {
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            var client = httpClientFactory.CreateClient("StripeClient");
            var secretKey = provider.GetRequiredService<IConfiguration>()["Stripe:SecretKey"];
            var stripeHttpClient = new SystemNetHttpClient(client);
            return new StripeClient(secretKey, apiBase: client.BaseAddress!.ToString(), httpClient: stripeHttpClient);
        });


        services.AddScoped<IStripeService, StripeService>();

        return services;
    }
}
