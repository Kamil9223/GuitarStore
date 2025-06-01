using Bogus;
using Domain.StronglyTypedIds;
using Domain.ValueObjects;
using Orders.Domain.Customers;
using Orders.Infrastructure.Database;

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
}
