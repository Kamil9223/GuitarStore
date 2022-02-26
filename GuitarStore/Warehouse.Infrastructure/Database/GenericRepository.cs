using Domain;
using System.Linq.Expressions;

namespace Warehouse.Infrastructure.Database;

internal class GenericRepository<TEntity> : IRepository<TEntity>
    where TEntity : Entity
{
    private readonly WarehouseDbContext _context;

    public GenericRepository(WarehouseDbContext context)
    {
        _context = context;
    }

    public async Task Add(TEntity entity)
    {
        await _context.Set<TEntity>().AddAsync(entity);
    }

    public Task<IEnumerable<TEntity>> Find(Expression<Func<TEntity, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public Task<TEntity> Get(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<TEntity>> GetAll()
    {
        throw new NotImplementedException();
    }

    public Task Remove(TEntity entity)
    {
        _context.Set<TEntity>().Remove(entity);
        return Task.CompletedTask;
    }
}
