using Auth.Core.Data;
using Catalog.Infrastructure.Database;
using Customers.Infrastructure.Database;
using GuitarStore.Api.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orders.Infrastructure.Database;
using Tests.EndToEnd.Setup.Modules.Common;
using Warehouse.Core.Database;
using Xunit;

namespace Tests.EndToEnd.Setup;
public class Application : IAsyncLifetime
{
    private readonly TestsContainers _containers = new();

    public IServiceProvider ServiceProvider => Factory!.Services;
    public WebApplicationFactory<Program> Factory { get; private set; } = null!;

    public Application() { }

    public async Task InitializeAsync()
    {
        await _containers.StartAsync();

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder
                .UseEnvironment("TestContainers")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration(config =>
                {
                    config.Sources.Clear();
                    config.Add(MemoryConfigurationTestSource.BuildConfiguration(_containers));
                })
                .ConfigureTestServices(ConfigureTestServices)
                //.ConfigureLogging(x => x.AddProvider(LoggerProvider))
            );

        // force starting web application
        // without this, the application is started when the first request is made or services are resolved from the integration test
        // When at least two tests are run in parallel, ConfigureTestServices might be called twice (looks like .NET bug)
        // it may cause singleton services to be created twice, which caused some issues with our "overriding services per test" support
        using var scope = Factory.Services.CreateScope();
        ValidateTestInfrastructureConfiguration(scope.ServiceProvider);
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }


    public GuitarStoreClient GetAuthorizedClient()
    {
        var httpClient = GetHttpClient();
        return new GuitarStoreClient(httpClient.BaseAddress!.ToString(), httpClient);
    }

    private void ConfigureTestServices(IServiceCollection services)
    {
        DbSetup.SetupAllModules(services, _containers.MsSqlContainerConnectionString);
        OverrideServicesSetup.SetupServicesOverrides(services);
    }

    public HttpClient GetHttpClient() => Factory!.CreateClient();

    public HttpClient GetHttpsClient(bool allowAutoRedirect = true) => Factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost:7028"),
        AllowAutoRedirect = allowAutoRedirect
    });

    private void ValidateTestInfrastructureConfiguration(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var configuredConnectionString = configuration.GetRequiredSection("ConnectionStrings:GuitarStore").Value;

        if (!string.Equals(configuredConnectionString, _containers.MsSqlContainerConnectionString, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "E2E tests are not using the SQL Server testcontainer connection string from configuration.");
        }

        EnsureConnectionStringMatches<AuthDbContext>(serviceProvider);
        EnsureConnectionStringMatches<CatalogDbContext>(serviceProvider);
        EnsureConnectionStringMatches<CustomersDbContext>(serviceProvider);
        EnsureConnectionStringMatches<OrdersDbContext>(serviceProvider);
        EnsureConnectionStringMatches<WarehouseDbContext>(serviceProvider);
    }

    private void EnsureConnectionStringMatches<TContext>(IServiceProvider serviceProvider)
        where TContext : DbContext
    {
        var dbContext = serviceProvider.GetRequiredService<TContext>();
        var actualConnectionString = dbContext.Database.GetConnectionString();

        if (!string.Equals(actualConnectionString, _containers.MsSqlContainerConnectionString, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"{typeof(TContext).Name} is not configured to use the SQL Server testcontainer.");
        }
    }
}
