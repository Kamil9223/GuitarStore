using Auth.Core.Events.Outgoing;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Tests.EndToEnd.Setup;
using Tests.EndToEnd.Setup.TestsHelpers;
using Xunit;

namespace Tests.EndToEnd.E2E_Customers.EventHandlers;

public sealed class UserRegisteredEventHandlerTest(Setup.Application app) : EndToEndTestBase(app)
{
    [Fact]
    public async Task Handle_WhenEventIsDeliveredTwice_ShouldCreateSingleCustomerAndCart()
    {
        var userId = Guid.NewGuid();
        var @event = new UserRegisteredEvent(
            UserId: userId,
            Email: $"customer-{Guid.NewGuid():N}@guitarstore.local",
            Name: "Repeat",
            LastName: "Customer",
            OccurredAtUtc: DateTimeOffset.UtcNow);

        RabbitMqChannel.PublishTestEvent(@event);
        RabbitMqChannel.PublishTestEvent(@event);

        await Waiter.WaitForCondition(async () =>
        {
            Databases.CustomersDbContext.ChangeTracker.Clear();
            return await Databases.CustomersDbContext.Customers.CountAsync(x => x.AuthUserId == userId) == 1;
        }, TimeSpan.FromSeconds(3));

        await Task.Delay(200);
        Databases.CustomersDbContext.ChangeTracker.Clear();

        var customer = await Databases.CustomersDbContext.Customers.SingleAsync(x => x.AuthUserId == userId);
        var cartsCount = await Databases.CustomersDbContext.Carts.CountAsync(x => x.CustomerId == customer.Id);

        customer.Name.ShouldBe(@event.Name);
        customer.LastName.ShouldBe(@event.LastName);
        customer.Email.Email.ShouldBe(@event.Email);
        cartsCount.ShouldBe(1);
    }
}
