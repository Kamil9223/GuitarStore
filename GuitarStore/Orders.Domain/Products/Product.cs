﻿using Domain;
using Domain.Exceptions;
using Domain.StronglyTypedIds;
using Domain.ValueObjects;

namespace Orders.Domain.Products;

public class Product : Entity
{
    public ProductId Id { get; }
    public string Name { get; }
    public Money Price { get; private set; }
    public uint Quantity { get; private set; }

    //For EF Core
    private Product() { }

    private Product(ProductId id, string name, decimal price, uint quantity)
    {
        Id = id;
        Name = name;
        Price = price;
        Quantity = quantity;
    }

    public static Product Create(ProductId id, string name, decimal price, uint quantity)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException($"Provided product name: [{name}] is not valid.");
        }

        if (quantity <= 0)
        {
            throw new DomainException($"Product quantity must be greater than zero.");
        }

        return new Product(id, name, price, quantity);
    }

    public void IncreaseQuantity(uint quantity) => Quantity += quantity;

    internal void DecreaseQuantity(uint quantity)
    {
        if (quantity > Quantity)
        {
            throw new DomainException($"Cannot decrease quantity of product.");
        }

        Quantity -= quantity;
    }

    internal bool IsQuantityDeacrisingPossible(uint quantity) => Quantity > quantity;
}
