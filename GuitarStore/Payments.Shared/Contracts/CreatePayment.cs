using Domain.ValueObjects;

namespace Payments.Shared.Contracts;
public sealed record CreatePaymentRequest(Currency Currency, decimal Amount);

public sealed record CreatePaymentResponse(string PaymentId);