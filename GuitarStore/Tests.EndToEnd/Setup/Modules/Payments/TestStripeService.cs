using Payments.Shared.Contracts;
using Payments.Shared.Services;

namespace Tests.EndToEnd.Setup.Modules.Payments;
internal class TestStripeService : IStripeService
{
    private Dictionary<Guid, Func<Task<CheckoutSessionResponse>>> _overrideCheckoutSessionBehaviours = [];

    public void AddCheckoutSessionBehavior(Guid behaviourKey, Func<Task<CheckoutSessionResponse>> behavior)
    {
        _overrideCheckoutSessionBehaviours.Add(behaviourKey, behavior);
    }

    public Guid CheckoutSessionBehaviorKey { get; set; }

    public Task<CheckoutSessionResponse> CreateCheckoutSession(CheckoutSessionRequest request, CancellationToken ct)
    {
        var behavior = _overrideCheckoutSessionBehaviours.GetValueOrDefault(CheckoutSessionBehaviorKey);
        return behavior?.Invoke() ?? Task.FromResult(new CheckoutSessionResponse
        {
            Url = "",
            SessionId = ""
        });
    }
}
