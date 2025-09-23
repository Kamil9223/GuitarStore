using Application.Contracts;
using Application.CQRS.Command;
using Application.CQRS.Query;
using Catalog.Application.Products.Commands;
using Catalog.Application.Products.Dtos;
using Catalog.Application.Products.Queries;
using Domain.StronglyTypedIds;
using Microsoft.AspNetCore.Mvc;

namespace GuitarStore.ApiGateway.Modules.Catalog.Products;

[ApiController]
[Route("products")]
public class ProductsController : ControllerBase
{
    private readonly ICommandHandlerExecutor _commandHandlerExecutor;
    private readonly IQueryHandlerExecutor _queryHandlerExecutor;

    public ProductsController(ICommandHandlerExecutor commandHandlerExecutor, IQueryHandlerExecutor queryHandlerExecutor)
    {
        _commandHandlerExecutor = commandHandlerExecutor;
        _queryHandlerExecutor = queryHandlerExecutor;
    }

    [HttpPost]
    public async Task<IActionResult> Create(AddProductCommand request)
    {
        await _commandHandlerExecutor.Execute(request);

        return Ok();
    }

    [HttpPost("list")]
    public async Task<IActionResult> GetList(ListProductsQuery request)
    {
        var products = await _queryHandlerExecutor
            .Execute<ListProductsQuery, PagedResponse<ProductBasedInfoDto>>(request);

        return Ok(products);
    }

    [HttpGet("{productId}")]
    public async Task<IActionResult> GetDetails([FromRoute] ProductId productId)
    {
        var productDetails = await _queryHandlerExecutor
            .Execute<ProductDetailsQuery, ProductDetailsDto>(new ProductDetailsQuery(productId));

        return Ok(productDetails);
    }
}
