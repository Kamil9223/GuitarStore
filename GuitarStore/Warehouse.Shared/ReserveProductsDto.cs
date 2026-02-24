using Domain.StronglyTypedIds;

namespace Warehouse.Shared;
public sealed record ReserveProductsDto(
    OrderId OrderId,
    TimeSpan TimeToLive,
    IReadOnlyCollection<ReserveProductDto> Products);

public sealed record ReserveProductDto(ProductId ProductId, int Quantity);


