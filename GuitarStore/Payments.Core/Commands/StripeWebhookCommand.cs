using Application.CQRS.Command;
using Common.Errors.Exceptions;
using Common.Outbox;
using Common.RabbitMq.Abstractions.EventHandlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Payments.Core.Events.Outgoing;
using Payments.Core.Services;
using Stripe;
using StripeConfiguration = Payments.Core.Services.StripeConfiguration;

namespace Payments.Core.Commands;

public sealed record StripeWebhookCommand(string Json, string Signature) : ICommand;

internal sealed class StripeWebhookCommandHandler(
    IWebhookIdempotencyStore webhookStore,
    IOutboxEventPublisher outboxEventPublisher,
    IPaymentIntentParser parser,
    IOptions<StripeConfiguration> stripeConfiguration,
    ILogger<StripeWebhookCommandHandler> logger,
    Database.PaymentsDbContext dbContext)
    : ICommandHandler<StripeWebhookCommand>
{
    public async Task Handle(StripeWebhookCommand command, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(command.Signature))
            throw new StripeSignatureException("Missing Stripe-Signature header");
        
        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                command.Json,
                command.Signature,
                stripeConfiguration.Value.WebhookSecret,
                throwOnApiVersionMismatch: false);
        }
        catch (StripeException ex)
        {
            logger.LogWarning(ex, "Stripe webhook signature verification failed.");
            throw new StripeSignatureException("Stripe webhook signature verification failed.");
        }
        
        var eventId = stripeEvent.Id ?? throw new InvalidOperationException("Stripe event id missing");
        var createdUtc = new DateTimeOffset(stripeEvent.Created.ToUniversalTime());

        // idempotency consume
        var consume = await webhookStore.TryConsumeAsync(eventId, createdUtc, ct);
        if (consume is IdempotencyConsumeResult.Duplicate)
        {
            logger.LogInformation("Stripe webhook duplicate event {EventId}. No-op.", eventId);
            return;
        }
        if (consume is IdempotencyConsumeResult.ExpiredNoOp)
        {
            logger.LogWarning("Stripe webhook event {EventId} stored but now expired. No-op.", eventId);
            return;
        }

        try
        {
            if (!parser.TryParse(stripeEvent, out var paymentIntentId, out var orderId))
            {
                logger.LogWarning("Stripe event {EventId} could not be parsed. Ignored.", eventId);
                await webhookStore.MarkCompletedAsync(eventId, ct);
                return;
            }

            switch (stripeEvent.Type)
            {
                case Stripe.Events.PaymentIntentSucceeded:
                    await outboxEventPublisher.PublishToOutbox(new OrderPaidEvent(orderId), ct);
                    break;

                case Stripe.Events.PaymentIntentPaymentFailed:
                    await outboxEventPublisher.PublishToOutbox(
                        new OrderPaymentFailedEvent(orderId, paymentIntentId, null, DateTime.UtcNow), ct);
                    break;

                case Stripe.Events.PaymentIntentCanceled:
                    // OrderCancelledEvent is now a business decision, not a payment outcome.
                    // Payment cancellation is logged but doesn't directly cancel the order.
                    logger.LogWarning(
                        "PaymentIntent {PaymentIntentId} for Order {OrderId} was canceled. No action taken.",
                        paymentIntentId, orderId);
                    await webhookStore.MarkCompletedAsync(eventId, ct);
                    return;

                default:
                    logger.LogInformation("Stripe webhook event {EventId} ignored: {Type}", eventId, stripeEvent.Type);
                    await webhookStore.MarkCompletedAsync(eventId, ct);
                    return;
            }

            await webhookStore.MarkCompletedAsync(eventId, ct);
            await dbContext.SaveChangesAsync(ct); //TODO: Provide TransactionWrapper
        }
        catch (Exception ex)
        {
            await webhookStore.MarkFailedAsync(eventId, ex.Message, ct);
            logger.LogError(ex, "Stripe webhook event {EventId} failed.", eventId);
            throw;
        }
    }
}