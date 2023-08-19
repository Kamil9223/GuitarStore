using Catalog.Application.Abstractions;
using Catalog.Application.Products.Dtos;
using Catalog.Application.Products.Queries;
using Catalog.Application.Products.Services;

namespace Catalog.Application.Products.QueryHandlers;

internal class GetProductsQueryHandler : IQueryHandler<ListProductsQuery, IEnumerable<ProductDto>>
{
    private readonly IProductQueryService _productQueryService;

    public GetProductsQueryHandler(IProductQueryService productQueryService)
    {
        _productQueryService = productQueryService;
    }

    public async Task<IEnumerable<ProductDto>> Handle(ListProductsQuery query)
    {
        var products = _productQueryService.Get();

        await Task.CompletedTask;

        return products!;
    }
}
