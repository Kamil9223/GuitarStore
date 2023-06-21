using Catalog.Domain.Products;
using System.Linq.Expressions;

namespace Catalog.Application.Products;

internal static class ProductSpecification
{
    internal static Expression<Func<Product, bool>> WithBrandAndName(string? brand, string? name)
        => product =>
            product.Brand == brand &&
            product.Name == name;
}
