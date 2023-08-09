namespace Catalog.Domain.IRepositories;

public interface IBrandRepository
{
    Task<Brand> Get(int id);
}
