using Catalog.Application.Products.Dtos;
using Catalog.Domain;
using Mapster;

namespace Catalog.Application.Products;

internal class ProductMappingRegisterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<IEnumerable<Product>, IEnumerable<ProductDto>>();

        config.NewConfig<Product, ProductDetailsDto>()
            .Map(dest => dest.Category, src => src.Category.CategoryName);
    }
}
