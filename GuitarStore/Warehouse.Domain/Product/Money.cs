using Domain;
using Warehouse.Domain.Product.Exceptions;

namespace Warehouse.Domain.Product;

public class Money : ValueObject
{
    private static readonly HashSet<string> AllowedValues = new()
    {
        "PLN",
        "EUR",
        "GBP"
    };

    public string Currency { get; set; }
    public decimal Value { get; set; }

    //For EF Core
    private Money() { }

    private Money(decimal value, string currency)
    {
        Value = value;
        Currency = currency;
    }

    public static Money Create(decimal value, string currency)
    {
        if (value < 0)
        {
            throw new PriceLessThanZeroException($"Price less than zero. Provided price = [{value}]");
        }

        if (!AllowedValues.Contains(currency))
        {
            throw new UnsupportedCurrencyException($"Provided currency: [{currency}] is not supported. Supported curencies: [{string.Join(", ", AllowedValues)}]");
        }

        return new Money(value, currency);
    }
}
