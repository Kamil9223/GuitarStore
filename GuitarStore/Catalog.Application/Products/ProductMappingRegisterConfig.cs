using Catalog.Application.Products.Dtos;
using Catalog.Domain.Products;
using Mapster;

namespace Catalog.Application.Products;

internal class ProductMappingRegisterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Product, ProductDetailsDto>()
            .Map(dest => dest.CategoryName, src => src.Category.CategoryName)
            ;
    }
}
