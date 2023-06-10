namespace Infrastructure.RabbitMq;

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

    public static implicit operator RabbitMqQueueName(string queueName) => new(queueName);
    public static implicit operator string(RabbitMqQueueName value) => value.QueueName;
}
