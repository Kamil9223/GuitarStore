using Application.Contracts;
using Application.CQRS.Query;
using Catalog.Application.Products.Dtos;
using Catalog.Application.Products.Services;

namespace Catalog.Application.Products.Queries;

public sealed record ListProductsQuery(
    int Limit,
    int Offset,
    ListProductsFilter Filter,
    ListProductsSort Sort) : IQuery;

public sealed record ListProductsFilter(
    string? Name,
    int? MinimumQuantity);

public sealed record ListProductsSort(
    SortType? Name,
    SortType? Price,
    SortType? Quantity);

internal sealed class ListProductsQueryHandler : IQueryHandler<ListProductsQuery, PagedResponse<ProductBasedInfoDto>>
{
    private readonly IProductQueryService _productQueryService;

    public ListProductsQueryHandler(IProductQueryService productQueryService)
    {
        _productQueryService = productQueryService;
    }

    public async Task<PagedResponse<ProductBasedInfoDto>> Handle(ListProductsQuery query)
    {
        var limitPlusOne = query.Limit + 1;
        var products = await _productQueryService.GetPaged(
            limitPlusOne,
            query.Offset,
            query.Filter,
            query.Sort,
            CancellationToken.None);

        var items = products.Count == limitPlusOne
            ? products.GetRange(0, query.Limit)
            : products;

        return new PagedResponse<ProductBasedInfoDto>
        {
            Items = items,
            Offset = query.Offset,
            Limit = query.Limit,
            HasMoreItems = products.Count == limitPlusOne
        };
    }
}
