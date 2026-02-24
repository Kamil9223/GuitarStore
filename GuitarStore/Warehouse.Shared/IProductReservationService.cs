using Domain.StronglyTypedIds;

namespace Warehouse.Shared;
public interface IProductReservationService
{
    Task ReserveProducts(ReserveProductsDto dto, CancellationToken ct);
    Task ConfirmReservations(OrderId orderId, CancellationToken ct);
    Task ReleaseReservations(OrderId orderId, CancellationToken ct);
}
