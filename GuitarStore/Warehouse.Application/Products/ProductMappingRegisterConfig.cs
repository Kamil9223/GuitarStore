using Mapster;
using Warehouse.Application.Products.Dtos;
using Warehouse.Domain.Products;

namespace Warehouse.Application.Products;

internal class ProductMappingRegisterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Product, ProductDetailsDto>()
            .Map(dest => dest.CategoryName, src => src.Category.CategoryName)
            ;
    }
}
