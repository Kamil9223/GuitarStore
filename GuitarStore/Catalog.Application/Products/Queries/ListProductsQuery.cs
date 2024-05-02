using Application.CQRS;
using Catalog.Application.Products.Dtos;
using Catalog.Application.Products.Services;

namespace Catalog.Application.Products.Queries;

public sealed record ListProductsQuery : IQuery;

internal sealed class ListProductsQueryHandler : IQueryHandler<ListProductsQuery, IEnumerable<ProductDto>>
{
    private readonly IProductQueryService _productQueryService;

    public ListProductsQueryHandler(IProductQueryService productQueryService)
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
