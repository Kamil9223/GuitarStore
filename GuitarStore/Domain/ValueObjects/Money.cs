namespace Domain.ValueObjects;

public class Money : ValueObject
{
    public decimal Value { get; set; }

    //For EF Core
    private Money() { }

    private Money(decimal value)
    {
        Value = value;
    }

    public static Money Create(decimal value)
    {
        if (value < 0)
        {
            throw new DomainException($"Price less than zero. Provided price = [{value}]");
        }

        return new Money(value);
    }

    public static implicit operator Money(decimal value) => new(value);
    public static implicit operator decimal(Money value) => value.Value;
    public static Money operator *(Money money, uint value) => new Money(money.Value * value);
    public static Money operator *(uint value, Money money) => new Money(money.Value * value);
    public static Money operator +(Money money1, Money money2) => money1.Value + money2.Value;
    public static Money operator -(Money money1, Money money2) => money1.Value - money2.Value;
}
