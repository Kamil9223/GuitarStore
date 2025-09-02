using Common.Errors.Exceptions;

namespace Domain.ValueObjects;
public sealed class Currency : ValueObject
{
    public string Value { get; init; }

    private static readonly IReadOnlyCollection<string> SupportedCurrencies = new[]
    {
        "USD", "EUR", "GBP", "JPY", "PLN"
    };

    public Currency() { }

    private Currency(string value)
    {
        Value = value;
    }

    public static Currency Create(string value)
    {
        if (SupportedCurrencies.Contains(value))
            return new Currency(value);

        throw DomainException.UnsupportedCurrency(value);
    }

    public static Currency PLN => new("PLN");

    public static implicit operator Currency(string value) => Create(value);

    public static implicit operator string(Currency value) => value.Value;
}
