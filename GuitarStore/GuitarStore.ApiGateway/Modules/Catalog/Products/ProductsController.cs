using Catalog.Application.Abstractions;
using Catalog.Application.Products.Commands;
using Catalog.Application.Products.Dtos;
using Catalog.Application.Products.Queries;
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

    [HttpPut("{productId}")]
    public async Task<IActionResult> Update(int productId, UpdateProductCommand request)
    {
        await _commandHandlerExecutor.Execute(request);

        return Ok();
    }

    [HttpDelete("{productId}")]
    public async Task<IActionResult> Delete(int productId)
    {
        await _commandHandlerExecutor.Execute(new DeleteProductCommand { Id = productId });

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _queryHandlerExecutor.Execute<ListProductsQuery, IEnumerable<ProductDto>>(new ListProductsQuery());

        return Ok(products);
    }

    [HttpGet("{productId}")]
    public async Task<IActionResult> GetDetails(int productId)
    {
        var productDetails = await _queryHandlerExecutor.Execute<ProductDetailsQuery, ProductDetailsDto>(new ProductDetailsQuery { ProductId = productId });

        return Ok(productDetails);
    }
}
