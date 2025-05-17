using Testcontainers.MsSql;
using Testcontainers.RabbitMq;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Tests.EndToEnd.Setup;
internal class TestsContainers : IAsyncDisposable
{
    private readonly MsSqlContainer _msSqlContainer;
    private readonly RabbitMqContainer _rabbitMqContainer;
    private readonly IContainer _stripeContainer;

    internal string MsSqlContainerConnectionString => _msSqlContainer.GetConnectionString();
    internal string RabbitMqContainerConnectionString => _rabbitMqContainer.GetConnectionString().Replace("127.0.0.1", "localhost");
    internal string StripeBaseUrl => "http://localhost:12111";

    internal TestsContainers()
    {
        _msSqlContainer = new MsSqlBuilder()
            .WithReuse(true)
            .WithPortBinding(15000)
            .WithLabel("reuse-id", "guitarStore_MSSQL")
            .Build();

        _rabbitMqContainer = new RabbitMqBuilder()
            .WithReuse(true)
            .WithPortBinding(15001)
            .WithLabel("reuse-id", "guitarStore_RabbitMQ")
            .Build();

        _stripeContainer = new ContainerBuilder()
            .WithImage("stripe/stripe-mock")
            .WithPortBinding(12111, 12111)
            .WithLabel("reuse-id", "guitarStore_Stripe")
            .Build();
    }

    public async Task StartAsync()
    {
        await Task.WhenAll(
            _msSqlContainer.StartAsync(),
            _rabbitMqContainer.StartAsync(),
            _stripeContainer.StartAsync()
        );
    }

    public async ValueTask DisposeAsync()
    {
        await _rabbitMqContainer.DisposeAsync();
        await _msSqlContainer.DisposeAsync();
        await _rabbitMqContainer.DisposeAsync();
    }
}
