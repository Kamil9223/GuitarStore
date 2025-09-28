using Common.RabbitMq.Abstractions;
using GuitarStore.Api.Client;
using Microsoft.Extensions.DependencyInjection;
using Tests.EndToEnd.Setup.Modules.Common;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Tests.EndToEnd.Setup;
public class EndToEndTestBase : IAsyncLifetime, IAssemblyFixture<Application>
{
    protected readonly Application _webApp;

    protected IServiceScope Scope { get; private set; } = null!;
    protected DbsAccessor Databases { get; private set; } = null!;
    protected TestContext TestContext { get; private set; } = null!;
    protected IRabbitMqChannel RabbitMqChannel { get; private set; } = null!;

    protected EndToEndTestBase(Application webApp)
    {
        _webApp = webApp;
    }

    public virtual Task InitializeAsync()
    {
        Scope = _webApp.ServiceProvider.CreateScope();
        Databases = new(Scope.ServiceProvider);
        TestContext = new(_webApp.GetAuthorizedClient());
        RabbitMqChannel = Scope.ServiceProvider.GetRequiredService<IRabbitMqChannel>();
        return Task.CompletedTask;
    }

    public virtual Task DisposeAsync()
    {
        Scope?.Dispose();
        return Task.CompletedTask;
    }
}

public sealed record TestContext(GuitarStoreClient GuitarStoreClient);
