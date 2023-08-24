using Domain;
using Domain.ValueObjects;

namespace Customers.Domain.Carts;

public class CartItem : Entity, IIdentifiable
{
    public int Id { get; }
    public int ProductId { get; set; }
    public string Name { get; }
    public Money Price { get; }
    public int Quantity { get; private set; }

    //For EF Core
    private CartItem() { }

    private CartItem(int productId, string name, Money price, int quantity)
    {
        ProductId = productId;
        Name = name;
        Price = price;
        Quantity = quantity;
    }

    internal static CartItem Create(int productId, string name, Money price, int quantity)
    {
        //Check rules

        return new CartItem(productId, name, price, quantity);
    }

    internal void IncreaseQuantity(int quantity) => Quantity += quantity;

    internal void DecreaseQuantity(int quantity)
    {
        if (quantity >= Quantity)
        {
            throw new DomainException($"Cannot decrease quantity of cartItem.");
        }

        Quantity -= quantity;
    }

    internal bool IsQuantityDeacrisingPossible(int quantity) => Quantity > quantity;
}
