﻿using Domain.StronglyTypedIds;
using System.Linq.Expressions;

namespace Customers.Domain.Customers;

public interface ICustomerRepository
{
    void Add(Customer customer);
    Task<bool> Exists(Expression<Func<Customer, bool>> predicate);
    Task<Customer> Get(CustomerId id);
}
