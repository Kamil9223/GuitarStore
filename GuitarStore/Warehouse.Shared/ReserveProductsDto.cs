namespace Warehouse.Shared;
public sealed record ReserveProductsDto(
    Guid OrderId,
    IReadOnlyCollection<ReserveProductDto> Products);

public sealed record ReserveProductDto(Guid ProductId, int Quantity);


