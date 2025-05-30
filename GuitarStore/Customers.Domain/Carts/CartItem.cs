﻿using Domain;
using Domain.Exceptions;
using Domain.StronglyTypedIds;
using Domain.ValueObjects;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Customers.Tests")]
namespace Customers.Domain.Carts;

public class CartItem : Entity
{
    public CartItemId Id { get; }
    public ProductId ProductId { get; }
    public string Name { get; }
    public Money Price { get; }
    public int Quantity { get; private set; }

    //For EF Core
    private CartItem() { }

    public CartItem(ProductId productId, string name, Money price, int quantity)
    {
        Id = CartItemId.New();
        ProductId = productId;
        Name = name;
        Price = price;
        Quantity = quantity;
    }

    internal static CartItem Create(ProductId productId, string name, Money price, int quantity)
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
