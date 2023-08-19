using Application.Exceptions;
using Catalog.Application.Abstractions;
using Catalog.Application.Products.Dtos;
using Catalog.Application.Products.Queries;
using Catalog.Application.Products.Services;

namespace Catalog.Application.Products.QueryHandlers;

internal class GetProductDetailsQueryHandler : IQueryHandler<ProductDetailsQuery, ProductDetailsDto>
{
    private readonly IProductQueryService _productQueryService;

    public GetProductDetailsQueryHandler(IProductQueryService productQueryService)
    {
        _productQueryService = productQueryService;
    }

    public async Task<ProductDetailsDto> Handle(ProductDetailsQuery query)
    {
        var product = await _productQueryService.Get(query.ProductId);
        if (product is null)
        {
            throw new NotFoundException($"Product with Id: [{query.ProductId}] not exists.");
        }

        return product;
    }
}
