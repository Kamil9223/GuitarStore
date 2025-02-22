using Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Payments.Shared.Contracts;
using Payments.Shared.Services;
using Stripe;

namespace Payments.Core.Services;
internal class StripeService : IStripeService
{
    private readonly IConfiguration _configuration;
    private readonly PaymentIntentService _intentService;
    private readonly PaymentMethodService _methodService;

    public StripeService(IConfiguration configuration, PaymentIntentService intentService, PaymentMethodService methodService)
    {
        _configuration = configuration;
        _intentService = intentService;

        StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        _methodService = methodService;
        //zapisac gdzieś klucze konfiguracyjne, appSettings ?
        //StripeConfiguration.ApiKey = _configuration["Stripe:PublicKey"];
    }

    public async Task<CreatePaymentResponse> CreatePaymentIntent(CreatePaymentRequest request)
    {
        var options = new PaymentIntentCreateOptions
        {
            Currency = request.Currency,
            Amount = (long)request.Amount,
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions() { Enabled = true }
        };

        var paymentIntent = await _intentService.CreateAsync(options);

        return new CreatePaymentResponse(paymentIntent.Id);
    }

    public async Task<ConfirmPaymentResponse> ConfirmPayment(ConfirmPaymentRequest request)
    {
        var stripePaymentResponse = await _intentService.ConfirmAsync(request.PaymentId, new PaymentIntentConfirmOptions
        {
            ReturnUrl = request.ReturnUrl,
            PaymentMethod = request.PaymentMethod == Shared.Contracts.PaymentMethod.Card
                ? "pm_card_visa"
                : request.PaymentMethod.ToString().ToLowerInvariant(),
        });

        return new ConfirmPaymentResponse(
            stripePaymentResponse.Status,
            stripePaymentResponse.LatestChargeId,
            stripePaymentResponse.PaymentMethodId);
    }

    public async Task<StripeList<Stripe.PaymentMethod>> Test()
    {
        var response = await _methodService.ListAsync();
        return response;
    }

    //webhoooki i potem testy. Wywalic kontroler, dodać komunikację z Orders i tak spróbować całość przejść narazie bez zapisu w bazie
    //potem pomyśleć o persystencji oraz retry'ach
}