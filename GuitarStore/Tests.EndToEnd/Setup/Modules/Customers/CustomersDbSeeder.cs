using Bogus;
using Customers.Domain.Carts;
using Customers.Domain.Customers;
using Customers.Domain.Products;
using Customers.Infrastructure.Carts;
using Customers.Infrastructure.Database;
using Domain.StronglyTypedIds;
using Domain.ValueObjects;
using Newtonsoft.Json;

namespace Tests.EndToEnd.Setup.Modules.Customers;
internal static class CustomersDbSeeder
{
    public static Customer SeedCustomer(
        this CustomersDbContext context,
        EmailAddress? emailAddress = null,
        string? name = null,
        string? lastName = null,
        CustomerAddress? customerAddress = null
        )
    {
        var faker = new Faker();
        var customer = Customer.Create(
            name ?? faker.Random.String2(30),
            lastName ?? faker.Random.String2(30),
            emailAddress ?? EmailAddress.Create(faker.Internet.Email()),
            customerAddress);
        context.Add(customer);
        return customer;
    }

    public static Product SeedProduct(
        this CustomersDbContext context,
        ProductId? productId = null,
        string? name = null,
        Money? amount = null,
        int? quantity = null)
    {
        var faker = new Faker();
        var product = Product.Create(
            productId ?? ProductId.New(),
            name ?? faker.Random.String2(30),
            amount ?? 100M,
            quantity ?? faker.Random.Int(min: 1, max: 10_000));
        context.Add(product);
        return product;
    }

    public static Cart SeedCart(
        this CustomersDbContext context,
        CustomerId customerId)
    {
        var cart = Cart.Create(customerId);
        var cartDbModel = new CartDbModel
        {
            CustomerId = cart.CustomerId,
            CartState = CartState.Empty,
            Object = JsonConvert.SerializeObject(cart)
        };
        context.Add(cartDbModel);
        return cart;
    }
}
