﻿using Application.Exceptions;
using Customers.Domain.Carts;
using Customers.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Customers.Infrastructure.Carts;
internal class CartRepository : ICartRepository
{
    private readonly CustomersDbContext _dbContext;

    public CartRepository(CustomersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Add(Cart cart)
    {
        var cartDbModel = new CartDbModel
        {
            CustomerId = cart.CustomerId,
            CartState = CartState.Empty,
            Object = JsonConvert.SerializeObject(cart)
        };

        _dbContext.Carts.Add(cartDbModel);
        await Task.CompletedTask;
    }

    public async Task Update(Cart cart)
    {
        var dbCart = await _dbContext.Carts.SingleOrDefaultAsync(x => x.CustomerId == cart.CustomerId);
        dbCart.Object = JsonConvert.SerializeObject(cart);
        dbCart.CartState = CartState.ContainingProducts;
    }

    public async Task Update(CheckoutCart cart)
    {
        var dbCart = await _dbContext.Carts.SingleOrDefaultAsync(x => x.CustomerId == cart.CustomerId);
        dbCart.Object = JsonConvert.SerializeObject(cart);
        dbCart.CartState = CartState.Checkouted;
    }

    public async Task<Cart> GetCart(int customerId)
    {
        var dbCart =  await _dbContext.Carts
            .Where(x => x.CustomerId == customerId)
            .Where(x => x.CartState == CartState.Empty || x.CartState == CartState.ContainingProducts)
            .SingleOrDefaultAsync();

        if (dbCart is null)
            throw new NotFoundException($"Cart with customerId: {customerId} not exists.");

        var cart = JsonConvert.DeserializeObject<Cart>(dbCart.Object);
        return cart;
    }
}
