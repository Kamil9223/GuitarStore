using System.Linq.Expressions;

namespace Domain;

public interface IRepository<TEntity> where TEntity : Entity
{
    Task<TEntity> Get(int id);
    Task<IEnumerable<TEntity>> GetAll();
    Task<IEnumerable<TEntity>> Find(Expression<Func<TEntity, bool>> predicate);
    Task Add(TEntity entity);
    Task Remove(TEntity entity);
}
