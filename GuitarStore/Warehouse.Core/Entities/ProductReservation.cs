using Common.Errors.Exceptions;
using Domain.StronglyTypedIds;

namespace Warehouse.Core.Entities;

public class ProductReservation
{
    public ProductReservation(
        OrderId orderId,
        ProductId productId,
        int reservedQuantity,
        TimeSpan timeToLive)
    {
        OrderId = orderId;
        ProductId = productId;
        ReservedQuantity = reservedQuantity;
        Status = ReservationStatus.Active;
        ExpiresAtUtc = DateTime.UtcNow.Add(timeToLive);
        CreatedAtUtc = DateTime.UtcNow;
    }
    
    public OrderId OrderId { get; init; }

    public ProductId ProductId { get; init; }

    public int ReservedQuantity { get; init; }
    
    public ReservationStatus Status { get; private set; }
    
    public DateTime ExpiresAtUtc { get; private set; }
    
    public DateTime CreatedAtUtc { get; init; }

    public void ConfirmReservation()
    {
        switch (Status)
        {
            case ReservationStatus.Confirmed:
                return;
            case ReservationStatus.Expired or ReservationStatus.Released:
                throw new DomainException($"Reservation for order: [{OrderId}] is already expired or released.");
            default:
                Status = ReservationStatus.Confirmed;
                break;
        }
    }

    public void ReleaseReservation(Stock relatedStock)
    {
        if (Status != ReservationStatus.Active)
            return;
        
        Status = ReservationStatus.Released;
        relatedStock.Quantity += ReservedQuantity;
    }

    public void ExpireReservation(Stock relatedStock)
    {
        if (Status != ReservationStatus.Active)
            return;
        
        Status = ReservationStatus.Expired;
        relatedStock.Quantity += ReservedQuantity;
    }
}

public enum ReservationStatus : byte
{
    Active = 1,
    Confirmed = 2,
    Released = 3,
    Expired = 4
}
