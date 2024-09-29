using Application.RabbitMq.Abstractions.Events;
using Infrastructure.RabbitMq.Abstractions;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using System.Reflection;

namespace Infrastructure.RabbitMq;

internal class RabbitMqSetupBackgroundService : IHostedService
{
    private readonly IRabbitMqConnector _rabbitMqConnector;

    public const string ExchangeName = "GuitarStore";

    public RabbitMqSetupBackgroundService(IRabbitMqConnector rabbitMqConnector)
    {
        _rabbitMqConnector = rabbitMqConnector;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _rabbitMqConnector.Connect();
            var channel = _rabbitMqConnector.CreateChannel();

            CreateExchange(channel);
            CreateQueues(channel);
            BindQueues(channel);
        }
        catch (Exception ex)
        {
            //Log errors
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _rabbitMqConnector.Dispose();
    }

    private void CreateExchange(IModel channel)
        => channel.ExchangeDeclare(
            exchange: ExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false);

    private void CreateQueues(IModel channel)
        => RabbitMqQueueName.DefinedQueuesNames.ForEach(queueName =>
        {
            channel.QueueDeclare(queue: queueName,
                                durable: true,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);
        });

    private void BindQueues(IModel channel)
    {
        var consumerEventType = typeof(IIntegrationConsumeEvent);

        var catalogConsumerEvents = GetModuleConsumerEvents("Catalog.Application", consumerEventType);
        var ordersConsumerEvents = GetModuleConsumerEvents("Orders.Application", consumerEventType);
        var customersConsumerEvents = GetModuleConsumerEvents("Customers.Application", consumerEventType);
        var authConsumerEvents = GetModuleConsumerEvents("Auth.Application", consumerEventType);
        var paymentsConsumerEvents = GetModuleConsumerEvents("Payments.Core", consumerEventType);
        var warehouseConsumerEvents = GetModuleConsumerEvents("Warehouse.Core", consumerEventType);

        BindQueues(RabbitMqQueueName.CatalogQueue, catalogConsumerEvents);
        BindQueues(RabbitMqQueueName.OrdersQueue, ordersConsumerEvents);
        BindQueues(RabbitMqQueueName.CustomersQueue, customersConsumerEvents);
        BindQueues(RabbitMqQueueName.AuthQueue, authConsumerEvents);
        BindQueues(RabbitMqQueueName.PaymentsQueue, paymentsConsumerEvents);
        BindQueues(RabbitMqQueueName.WarehouseQueue, warehouseConsumerEvents);

        IEnumerable<Type> GetModuleConsumerEvents(string appModuleName, Type consumerEventType)
            => Assembly
                .Load(new AssemblyName(appModuleName))
                .GetTypes()
                .Where(p => p.IsAssignableTo(consumerEventType));

        void BindQueues(string queueName, IEnumerable<Type> moduleConsumerEventsTypes)
        {
            foreach (var moduleConsumerEventType in moduleConsumerEventsTypes)
            {
                channel.QueueBind(queue: queueName,
                             exchange: ExchangeName,
                             routingKey: moduleConsumerEventType.Name);
            }
        }
    }
}
