using Application.CQRS;

namespace Customers.Application.Carts.Commands;
public class CheckoutCartCommand : ICommand
{
    public int CustomerId { get; init; }
    public PaymentCommandPart Payment { get; init; }
    public DeliveryCommandPart Delivery { get; init; }


    public class PaymentCommandPart
    {
        public int PaymentId { get; init; }
        public string PaymentType { get; init; }
    }

    public class DeliveryCommandPart
    {
        public int DelivererId { get; init; }
        public string Deliverer { get; init; }
    }
}
