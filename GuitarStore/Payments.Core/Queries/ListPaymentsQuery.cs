using Application.CQRS;
using Payments.Core.Dtos;
using Payments.Shared.Services;

namespace Payments.Core.Queries;
public sealed record ListPaymentsQuery(string Currency, long Amount) : IQuery;

internal sealed class ListPaymentsQueryHandler : IQueryHandler<ListPaymentsQuery, PaymentsListDto>
{
    private readonly IStripeService _stripeService;

    public ListPaymentsQueryHandler(IStripeService stripeService)
    {
        _stripeService = stripeService;
    }

    public async Task<PaymentsListDto> Handle(ListPaymentsQuery query)
    {
        //await _stripeService.CreatePaymentIntent(query.Currency, query.Amount);

        return new PaymentsListDto();
    }
}
