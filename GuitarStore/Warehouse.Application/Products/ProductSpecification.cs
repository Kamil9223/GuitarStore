using System.Linq.Expressions;
using Warehouse.Domain.Products;

namespace Warehouse.Application.Products;

internal static class ProductSpecification
{
    internal static Expression<Func<Product, bool>> WithBrandAndName(string? brand, string? name)
        => product =>
            product.Brand == brand &&
            product.Name == name;
}
