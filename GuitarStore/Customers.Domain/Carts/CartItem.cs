using Common.Errors.Exceptions;
using Domain;
using Domain.StronglyTypedIds;
using Domain.ValueObjects;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Customers.Tests")]
namespace Customers.Domain.Carts;

public class CartItem : Entity
{
    public CartItemId Id { get; init; }
    public ProductId ProductId { get; init; }
    public string Name { get; init; }
    public Money Price { get; init; }
    public int Quantity { get; set; }

    //For EF Core
    private CartItem() { }

    public CartItem(CartItemId id, ProductId productId, string name, Money price, int quantity)
    {
        Id = id;
        ProductId = productId;
        Name = name;
        Price = price;
        Quantity = quantity;
    }

    internal static CartItem Create(ProductId productId, string name, Money price, int quantity)
    {
        //Check rules

        return new CartItem(CartItemId.New(), productId, name, price, quantity);
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
