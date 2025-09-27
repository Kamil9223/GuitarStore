using Bogus;
using Domain.StronglyTypedIds;
using Domain.ValueObjects;
using Orders.Domain.Customers;
using Orders.Domain.Orders;
using Orders.Infrastructure.Database;
using Orders.Infrastructure.Orders;

namespace Tests.EndToEnd.Setup.Modules.Orders;
internal static class OrdersDbSeeder
{
    public static Customer SeedCustomer(
        this OrdersDbContext context,
        CustomerId customerId,
        EmailAddress? emailAddress = null,
        string? name = null,
        string? lastName = null
        )
    {
        var faker = new Faker();
        var customer = Customer.Create(
            customerId,
            name ?? faker.Random.String2(30),
            lastName ?? faker.Random.String2(30),
            emailAddress ?? EmailAddress.Create(faker.Internet.Email()));
        context.Add(customer);
        return customer;
    }

    public static OrderReadModel SeedOrderReadModel(
        this OrdersDbContext context,
        CustomerId customerId,
        OrderStatus? status = null,
        decimal? totalPrice = null,
        int? itemsCount = null,
        DeliveryAddress? deliveryAddress = null
        )
    {
        var faker = new Faker();
        var order = new OrderReadModel
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId.Value,
            Status = status.HasValue ? (byte)status : (byte)OrderStatus.New,
            CreatedAt = DateTime.UtcNow,
            TotalPrice = totalPrice ?? faker.Finance.Amount(1, 1000, 2),
            ItemsCount = itemsCount ?? faker.Random.Int(1, 1000),
            Deliverer = faker.Random.String2(20),
            UpdatedAt = DateTime.UtcNow,
            DeliveryAddress = deliveryAddress ?? new DeliveryAddress(
            
                country: faker.Random.String2(30),
                houseNumber: faker.Random.String2(10),
                localityName: faker.Random.String2(30),
                localNumber: faker.Random.String2(10),
                postalCode: faker.Random.String2(6),
                street: faker.Random.String2(30)
            )
        };
        context.Add(order);
        return order;
    }
}
