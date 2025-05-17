using GuitarStore.Api.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Tests.EndToEnd.Setup.Modules.Common;
using Xunit;

namespace Tests.EndToEnd.Setup;
public class Application : IAsyncLifetime
{
    private readonly TestsContainers _containers = new();
    private WebApplicationFactory<Program> _app = null!;

    public IServiceProvider ServiceProvider => _app!.Services;

    public Application() { }

    public async Task InitializeAsync()
    {
        await _containers.StartAsync();

        _app = new WebApplicationFactory<Program>()
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
        using var scope = _app.Services.CreateScope();
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
        // sql context
        DbSetup.SetupAllModules(services, _containers.MsSqlContainerConnectionString);

        // eventing
        //services.AddRabbitMqInfraServices(new RabbitMqSettings
        //{
        //    Enabled = true,
        //    ConnectionString = _webFixture.RabbitMqContainer!.GetConnectionString(),
        //    Exchange = "Mrp.events",
        //    ConsumptionQueue = "Mrp.events.consumer.self"
        //});

        //services.AddOverrides();
        //services.AddTestRunIdMiddleware();
    }

    public HttpClient GetHttpClient() => _app!.CreateClient();
}
