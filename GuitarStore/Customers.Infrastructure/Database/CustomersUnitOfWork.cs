using Common.EfCore.Transactions;
using Customers.Application.Abstractions;

namespace Customers.Infrastructure.Database;
internal class CustomersUnitOfWork(CustomersDbContext dbContext) 
    : UnitOfWork<CustomersDbContext>(dbContext), ICustomersUnitOfWork
{
}
