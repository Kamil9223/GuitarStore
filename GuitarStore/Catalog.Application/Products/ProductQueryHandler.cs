using Application.Exceptions;
using Catalog.Application.Abstractions;
using Catalog.Application.Products.Dtos;
using Catalog.Application.Products.Queries;
using Catalog.Domain.Products;
using Mapster;

namespace Catalog.Application.Products;

internal class ProductQueryHandler :
    IQueryHandler<ProductDetailsQuery, ProductDetailsDto>,
    IQueryHandler<ListProductsQuery, IEnumerable<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public ProductQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductDetailsDto> Handle(ProductDetailsQuery query)
    {
        var product = await _productRepository.GetProduct(query.ProductId);
        if (product is null)
        {
            throw new NotFoundException($"Product with Id: [{query.ProductId}] not exists.");
        }

        return product.Adapt<ProductDetailsDto>();
    }

    public async Task<IEnumerable<ProductDto>> Handle(ListProductsQuery query)
    {
        var products = await _productRepository.GetAll();

        return products.Adapt<IEnumerable<ProductDto>>();
    }
}
