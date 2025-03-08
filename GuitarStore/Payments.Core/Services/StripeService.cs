using Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Payments.Shared.Contracts;
using Payments.Shared.Services;
using Stripe;
using Stripe.Checkout;

namespace Payments.Core.Services;
internal class StripeService : IStripeService
{
    private readonly IConfiguration _configuration;
    private readonly SessionService _sessionService;

    public StripeService(
        IConfiguration configuration,
        SessionService sessionService)
    {
        _configuration = configuration;
        _sessionService = sessionService;
        StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];     
        //zapisac gdzieś klucze konfiguracyjne, appSettings ?
        //StripeConfiguration.ApiKey = _configuration["Stripe:PublicKey"];
    }

    public async Task<string> CreateCheckoutSession(CheckoutSessionRequest request)
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
                        UnitAmountDecimal = x.Price,
                    }
                })
                .ToList(),
            SuccessUrl = "https://www.youtube.com",//for tests
            CancelUrl = "https://www.google.com", //for tests
            Mode = "payment"
        };

        var session = await _sessionService.CreateAsync(options);
        return session.Url;       
    }

    //webhoooki i potem testy. Wywalic kontroler, dodać komunikację z Orders i tak spróbować całość przejść narazie bez zapisu w bazie
    //potem pomyśleć o persystencji oraz retry'ach
}