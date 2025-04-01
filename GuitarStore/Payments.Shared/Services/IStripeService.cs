using Payments.Shared.Contracts;

namespace Payments.Shared.Services;
public interface IStripeService
{
    Task<CheckoutSessionResponse> CreateCheckoutSession(CheckoutSessionRequest request);
}
