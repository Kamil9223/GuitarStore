using Domain.ValueObjects;
using Payments.Shared.Contracts;
using Stripe;

namespace Payments.Shared.Services;
public interface IStripeService
{
    Task<CreatePaymentResponse> CreatePaymentIntent(CreatePaymentRequest request);

    Task<ConfirmPaymentResponse> ConfirmPayment(ConfirmPaymentRequest request);

    Task<StripeList<Stripe.PaymentMethod>> Test();
}
