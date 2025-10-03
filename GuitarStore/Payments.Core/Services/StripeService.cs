using Payments.Shared.Contracts;
using Payments.Shared.Services;
using Stripe;
using Stripe.Checkout;

namespace Payments.Core.Services;
internal class StripeService : IStripeService
{
    private readonly SessionService _sessionService;

    public StripeService(StripeClient stripeClient)
    {
        _sessionService = new SessionService(stripeClient);
    }

    public async Task<CheckoutSessionResponse> CreateCheckoutSession(CheckoutSessionRequest request, CancellationToken ct)
    {
        var options = new SessionCreateOptions
        {
            LineItems = request.Products
                .Select(x => new SessionLineItemOptions
                {
                    Quantity = x.Quantity,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = x.Currency,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = x.Name
                        },
                        UnitAmountDecimal = x.Amount,
                    }
                })
                .ToList(),
            SuccessUrl = "https://www.youtube.com",//for tests
            CancelUrl = "https://www.google.com", //for tests
            Mode = "payment",
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    { IStripeService.OrderIdMetadataKey, request.OrderId.ToString() }
                }
            }
        };

        try
        {
            var session = await _sessionService.CreateAsync(options, cancellationToken: ct);
            return new CheckoutSessionResponse
            {
                Url = session.Url,
                SessionId = session.Id,
            };
        }
        catch (Exception ex)
        {
            throw new Exception();//W niektorych przypadkach blad, w niektorych response z url do strony gdzie jest info ze platnosc
            //nie poszla (bo 500 np) wtedy order stworzony ale nieoplacony, bedzie retry np za godzine i za 6 i na drugi dzien. jak nie pojdzie
            //to notyfikacja ze anulujemy to zamowienie i sorry
        }
    }
}