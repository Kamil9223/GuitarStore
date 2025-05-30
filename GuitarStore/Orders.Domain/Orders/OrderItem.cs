﻿using Domain;
using Domain.StronglyTypedIds;

namespace Orders.Domain.Orders;

public class OrderItem : Entity
{
    public Guid Id { get; }
    public string Name { get; }
    public decimal Price { get; }
    public int Quantity { get; }
    public ProductId ProductId { get; }

    private OrderItem(string name, decimal price, int quantity, ProductId productId)
    {
        Id = Guid.NewGuid();
        Name = name;
        Price = price;
        Quantity = quantity;
        ProductId = productId;
    }

    public static OrderItem Create(string name, decimal price, int quantity, ProductId productId)
    {
        return new OrderItem(name, price, quantity, productId);
    }
}
