using Microsoft.AspNetCore.Mvc;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Products.Commands;

namespace GuitarStore.ApiGateway.Modules.Warehouse.Products;

[ApiController]
[Route("products")]
public class ProductsController : ControllerBase
{
    private readonly ICommandHandlerExecutor<AddProductCommand> _addProductCommandHandler;
    private readonly ICommandHandlerExecutor<DeleteProductCommand> _deleteProductCommandHandler;
    private readonly ICommandHandlerExecutor<UpdateProductCommand> _updateProductCommandHandler;

    public ProductsController(
        ICommandHandlerExecutor<AddProductCommand> addProductCommandHandler,
        ICommandHandlerExecutor<DeleteProductCommand> deleteProductCommandHandler,
        ICommandHandlerExecutor<UpdateProductCommand> updateProductCommandHandler)
    {
        _addProductCommandHandler = addProductCommandHandler;
        _deleteProductCommandHandler = deleteProductCommandHandler;
        _updateProductCommandHandler = updateProductCommandHandler;
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
}
