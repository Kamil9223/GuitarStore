using Application.RabbitMq.Abstractions;
using Application.RabbitMq.Abstractions.Events;
using Domain.StronglyTypedIds;
using Domain.ValueObjects;
using Payments.Shared.Contracts;
using Payments.Shared.Services;
using PaymentMethod = Payments.Shared.Contracts.PaymentMethod;

namespace Payments.Core.Events.Incoming;
internal sealed record CreatedOrderEvent(
    OrderId OrderId,
    decimal TotalAmount,
    Currency Currency,
    PaymentMethod PaymentMethod) : IntegrationEvent, IIntegrationConsumeEvent;

internal sealed class CreatedOrderEventHandler : IIntegrationEventHandler<CreatedOrderEvent>
{
    private readonly IStripeService _stripeService;

    public CreatedOrderEventHandler(IStripeService stripeService)
    {
        _stripeService = stripeService;
    }

    public async Task Handle(CreatedOrderEvent @event)
    {
        var paymentIntentRequest = new CreatePaymentRequest(@event.Currency, @event.TotalAmount);
        var paymentIntentResponse = await _stripeService.CreatePaymentIntent(paymentIntentRequest);

        var confirmPaymentRequest = new ConfirmPaymentRequest(
            paymentIntentResponse.PaymentId,
            "return_URL",
            @event.PaymentMethod);
        var confirmPaymentResponse = await _stripeService.ConfirmPayment(confirmPaymentRequest);
    }
}
