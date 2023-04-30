using System.Linq.Expressions;
using Warehouse.Domain.Categories;

namespace Warehouse.Application.Categories;

internal static class CategorySpecification
{
    internal static Expression<Func<Category, bool>> IsTheMostNestedCategory(int Id)
        => category =>
            category.Id == Id &&
            category.SubCategories != null &&
            !category.SubCategories.Any();
}
