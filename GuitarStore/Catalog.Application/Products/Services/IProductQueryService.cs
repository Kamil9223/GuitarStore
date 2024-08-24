using Catalog.Application.Products.Dtos;
using Domain.StronglyTypedIds;

namespace Catalog.Application.Products.Services;

public interface IProductQueryService
{
    Task<ProductDetailsDto?> Get(ProductId id);
    IEnumerable<ProductDto?> Get();
    Task<IReadOnlyCollection<ProductBasedInfoDto>> GetAll();
}
