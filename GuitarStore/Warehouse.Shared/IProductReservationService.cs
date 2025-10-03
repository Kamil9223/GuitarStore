namespace Warehouse.Shared;
public interface IProductReservationService
{
    Task ReserveProduct(ReserveProductsDto dto, CancellationToken ct);
}
