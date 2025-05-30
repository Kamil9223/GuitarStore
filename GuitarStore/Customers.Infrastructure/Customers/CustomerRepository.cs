﻿using Customers.Domain.Customers;
using Customers.Infrastructure.Database;
using Domain.Exceptions;
using Domain.StronglyTypedIds;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Customers.Infrastructure.Customers;

internal class CustomerRepository : ICustomerRepository
{
    private readonly CustomersDbContext _customersDbContext;

    public CustomerRepository(CustomersDbContext customersDbContext)
    {
        _customersDbContext = customersDbContext;
    }

    public void Add(Customer customer)
    {
        _customersDbContext.Customers.Add(customer);
    }

    public async Task<bool> Exists(Expression<Func<Customer, bool>> predicate)
    {
        return await _customersDbContext.Customers.AnyAsync(predicate);
    }

    public async Task<Customer> Get(CustomerId id) =>
        await _customersDbContext.Customers.SingleOrDefaultAsync(x => x.Id == id)
        ?? throw new NotFoundException(id);
}
