using Payments.Shared.Contracts;

namespace Payments.Shared.Services;
public interface IStripeService
{
    Task<string> CreateCheckoutSession(CheckoutSessionRequest request);
}
