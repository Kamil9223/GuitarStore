using Microsoft.Extensions.Configuration.Memory;

namespace Tests.EndToEnd.Setup;
internal static class MemoryConfigurationTestSource
{
    internal static MemoryConfigurationSource BuildConfiguration(TestsContainers containers)
    {
        return new MemoryConfigurationSource
        {
            InitialData =
            [
                new("ConnectionStrings:GuitarStore", containers.MsSqlContainerConnectionString),
                new("ConnectionStrings:RabbitMq", containers.RabbitMqContainerConnectionString)
            ]
        };
    }
}
