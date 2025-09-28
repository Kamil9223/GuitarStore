namespace Common.RabbitMq.Abstractions;

public class RabbitMqQueueName
{
    public readonly string QueueName;

    public RabbitMqQueueName(string queueName)
    {
        QueueName = queueName;
    }

    public static RabbitMqQueueName AuthQueue => new RabbitMqQueueName("GuitarStore.AuthQueue");
    public static RabbitMqQueueName CustomersQueue => new RabbitMqQueueName("GuitarStore.CustomersQueue");
    public static RabbitMqQueueName OrdersQueue = new RabbitMqQueueName("GuitarStore.OrdersQueue");
    public static RabbitMqQueueName CatalogQueue = new RabbitMqQueueName("GuitarStore.CatalogQueue");
    public static RabbitMqQueueName PaymentsQueue = new RabbitMqQueueName("GuitarStore.PaymentsQueue");
    public static RabbitMqQueueName WarehouseQueue = new RabbitMqQueueName("GuitarStore.WarehouseQueue");
    public static RabbitMqQueueName DeliveryQueue = new RabbitMqQueueName("GuitarStore.DeliveryQueue");

    public static implicit operator RabbitMqQueueName(string queueName) => new(queueName);
    public static implicit operator string(RabbitMqQueueName value) => value.QueueName;

    public static List<RabbitMqQueueName> DefinedQueuesNames
        => [
            AuthQueue,
            CustomersQueue,
            OrdersQueue,
            CatalogQueue,
            PaymentsQueue,
            WarehouseQueue,
            DeliveryQueue
        ];
}
