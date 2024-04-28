namespace Customers.Shared;

public class CheckoutCartDto
{
    public int CustomerId { get; init; }
    public int PaymentId { get; init; }
    public string PaymentType { get; init; }
    public int DelivererId { get; init; }
    public string Deliverer {  get; init; }
    public Address DeliveryAddress { get; init; }
    public IReadOnlyCollection<CheckoutCartItem> Items { get; init; }

    public class Address
    {
        public string Country { get; init; }
        public string LocalityName { get; init; }
        public string PostalCode { get; init; }
        public string HouseNumber { get; init; }
        public string Street { get; init; }
        public string LocalNumber { get; init; }
    }

    public class CheckoutCartItem
    {
        public string Name { get; init; }
        public decimal Price { get; init; }
        public uint Quantity { get; init; }
    }
}
