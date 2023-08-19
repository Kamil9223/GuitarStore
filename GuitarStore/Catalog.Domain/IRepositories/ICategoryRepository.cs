namespace Catalog.Domain.IRepositories;

public interface ICategoryRepository
{
    Task<Category?> GetCategoryThatHasNotChildren(int id);
}
