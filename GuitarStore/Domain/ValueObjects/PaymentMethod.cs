namespace Domain.ValueObjects;

public enum PaymentMethod : byte //TODO: wynieść do commona, moze to powinno być jako PaymentId domanowy, a dostęne metody z bazy albo configa
{
    Card = 1,
    Blik = 2,
    Link = 3
}
