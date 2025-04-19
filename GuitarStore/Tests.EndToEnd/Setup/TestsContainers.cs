using Testcontainers.MsSql;
using Testcontainers.RabbitMq;

namespace Tests.EndToEnd.Setup;
internal class TestsContainers : IAsyncDisposable
{
    private readonly MsSqlContainer _msSqlContainer;
    private readonly RabbitMqContainer _rabbitMqContainer;

    internal string MsSqlContainerConnectionString => _msSqlContainer.GetConnectionString();
    internal string RabbitMqContainerConnectionString => _rabbitMqContainer.GetConnectionString();

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
    }

    public async Task StartAsync()
    {
        await Task.WhenAll(
            _msSqlContainer.StartAsync(),
            _rabbitMqContainer.StartAsync()
        );
    }

    public async ValueTask DisposeAsync()
    {
        await _rabbitMqContainer.DisposeAsync();
        await _msSqlContainer.DisposeAsync();
    }
}
