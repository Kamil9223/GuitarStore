using Domain;
using Domain.ValueObjects;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Customers.Tests")]
namespace Customers.Domain.Carts;

public class CartItem : Entity, IIdentifiable
{
    public int Id { get; }
    public int ProductId { get; }
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
        if (!IsDecreasingQuantityPossible(quantity))
        {
            throw new DomainException($"Cannot decrease quantity by [{quantity}] because CartItem quantity = [{Quantity}]");
        }

        Quantity -= quantity;
    }

    internal bool IsDecreasingQuantityPossible(int quantity) => Quantity > quantity;
}
