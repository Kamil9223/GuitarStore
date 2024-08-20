using Domain.StronglyTypedIds;

namespace Warehouse.Shared;
public sealed record ReserveProductsDto(
    OrderId OrderId,
    IReadOnlyCollection<ReserveProductDto> Products);

public sealed record ReserveProductDto(ProductId ProductId, int Quantity);


