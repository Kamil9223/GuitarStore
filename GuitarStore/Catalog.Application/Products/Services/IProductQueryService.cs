using Catalog.Application.Products.Dtos;

namespace Catalog.Application.Products.Services;

public interface IProductQueryService
{
    Task<ProductDetailsDto?> Get(int id);
    IEnumerable<ProductDto?> Get();
    Task<IReadOnlyCollection<ProductBasedInfoDto>> GetAll();
}
