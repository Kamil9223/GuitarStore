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
    private readonly ICommandHandlerExecutor<AddProductCommand> _addProductCommandHandler;
    private readonly ICommandHandlerExecutor<DeleteProductCommand> _deleteProductCommandHandler;
    private readonly ICommandHandlerExecutor<UpdateProductCommand> _updateProductCommandHandler;
    private readonly IQueryHandler<ListProductsQuery, IEnumerable<ProductDto>> _listProductsQueryHandler;
    private readonly IQueryHandler<ProductDetailsQuery, ProductDetailsDto> _productDetailsQueryHandler;

    public ProductsController(
        ICommandHandlerExecutor<AddProductCommand> addProductCommandHandler,
        ICommandHandlerExecutor<DeleteProductCommand> deleteProductCommandHandler,
        ICommandHandlerExecutor<UpdateProductCommand> updateProductCommandHandler,
        IQueryHandler<ListProductsQuery, IEnumerable<ProductDto>> listProductsQueryHandler,
        IQueryHandler<ProductDetailsQuery, ProductDetailsDto> productDetailsQueryHandler)
    {
        _addProductCommandHandler = addProductCommandHandler;
        _deleteProductCommandHandler = deleteProductCommandHandler;
        _updateProductCommandHandler = updateProductCommandHandler;
        _listProductsQueryHandler = listProductsQueryHandler;
        _productDetailsQueryHandler = productDetailsQueryHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Create(AddProductCommand request)
    {
        await _addProductCommandHandler.Execute(request);

        return Ok();
    }

    [HttpPut("{productId}")]
    public async Task<IActionResult> Update(int productId, UpdateProductCommand request)
    {
        await _updateProductCommandHandler.Execute(request);

        return Ok();
    }

    [HttpDelete("{productId}")]
    public async Task<IActionResult> Delete(int productId)
    {
        await _deleteProductCommandHandler.Execute(new DeleteProductCommand { Id = productId });

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _listProductsQueryHandler.Handle(new ListProductsQuery());

        return Ok(products);
    }

    [HttpGet("{productId}")]
    public async Task<IActionResult> GetDetails(int productId)
    {
        var productDetails = await _productDetailsQueryHandler.Handle(new ProductDetailsQuery { ProductId = productId });

        return Ok(productDetails);
    }
}
