using Catalog.Domain.Categories;
using System.Linq.Expressions;

namespace Catalog.Application.Categories;

internal static class CategorySpecification
{
    internal static Expression<Func<Category, bool>> IsTheMostNestedCategory(int Id)
        => category =>
            category.Id == Id &&
            category.SubCategories != null &&
            !category.SubCategories.Any();
}
