using Bogus;
using Customers.Domain.Carts;
using Customers.Domain.Customers;
using Customers.Domain.Products;
using Customers.Infrastructure.Carts;
using Customers.Infrastructure.Database;
using Domain.StronglyTypedIds;
using Domain.ValueObjects;
using Newtonsoft.Json;
using CartsDelivery = Customers.Domain.Carts.Delivery;

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
            customerAddress ?? customerAddress);
        context.Add(customer);
        return customer;
    }

    public static CustomerAddress SeedAddress(this CustomersDbContext context)
    {
        var faker = new Faker();
        var address = CustomerAddress.Create(
            country: faker.Random.String2(20),
            locality: faker.Random.Enum<Locality>(),
            localityName: faker.Random.String2(20),
            postalCode: faker.Random.String2(10),
            houseNumber: faker.Random.String2(20),
            street: faker.Random.String2(50),
            localNumber: faker.Random.String2(20));
        return address;
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

    public static CartDbModel SeedCart(
        this CustomersDbContext context,
        CustomerId customerId,
        CartState? cartState = null,
        IReadOnlyCollection<CartItem>? items = null)
    {
        var cart = Cart.Create(customerId, items);
        var cartDbModel = new CartDbModel
        {
            CustomerId = cart.CustomerId,
            CartState = cartState ?? CartState.Empty,
            Object = JsonConvert.SerializeObject(cart)
        };
        context.Add(cartDbModel);
        return cartDbModel;
    }

    public static CartDbModel SeedCheckoutCart(
        this CustomersDbContext context,
        CustomerId customerId,
        IReadOnlyCollection<CartItem> items,
        CartsDelivery? delivery = null)
    {
        var cart = Cart.Create(customerId, items);
        var checkout = cart.Checkout();
        if (delivery is not null)
            checkout.SetModelOfDelivery(delivery);
        var cartDbModel = new CartDbModel
        {
            CustomerId = cart.CustomerId,
            CartState = CartState.Checkouted,
            Object = JsonConvert.SerializeObject(checkout)
        };
        context.Add(cartDbModel);
        return cartDbModel;
    }
}
