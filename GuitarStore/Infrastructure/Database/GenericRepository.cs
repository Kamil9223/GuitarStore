using Domain;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Database;

public class GenericRepository<TEntity> : IRepository<TEntity>
    where TEntity : Entity
{
    protected readonly DbContext _context;

    public GenericRepository(DbContext context)
    {
        _context = context;
    }

    public async Task Add(TEntity entity)
    {
        await _context.Set<TEntity>().AddAsync(entity);
    }

    public async Task<IEnumerable<TEntity?>> Find(Expression<Func<TEntity, bool>> predicate)
    {
        return await _context.Set<TEntity>().Where(predicate).ToListAsync();
    }

    public async Task<TEntity?> SingleOrDefault(Expression<Func<TEntity, bool>> predicate)
    {
        return await _context.Set<TEntity>().SingleOrDefaultAsync(predicate);
    }

    public async Task<bool> Any(Expression<Func<TEntity, bool>> predicate)
    {
        return await _context.Set<TEntity>().AnyAsync(predicate);
    }

    public async Task<TEntity?> Get(int id)
    {
        return await _context.Set<TEntity>().FindAsync(id);
    }

    public async Task<IEnumerable<TEntity?>> GetAll()
    {
        return await _context.Set<TEntity>().ToListAsync();
    }

    public Task Remove(TEntity entity)
    {
        _context.Set<TEntity>().Remove(entity);
        return Task.CompletedTask;
    }
}
