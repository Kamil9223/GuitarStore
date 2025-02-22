namespace Payments.Shared.Contracts;
public sealed record ConfirmPaymentRequest(
    string PaymentId,
    string ReturnUrl,
    PaymentMethod PaymentMethod);

public sealed record ConfirmPaymentResponse(
    string Status,
    string ChargedId,
    string PaymentMethodId);

public enum PaymentMethod : byte
{
    Card = 1,
    Blik = 2,
    Link = 3
}
