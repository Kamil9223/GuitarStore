namespace Orders.Application.Configuration;

public sealed record OrdersConfiguration
{
    /// <summary>
    /// Reservation time to live (in minutes)
    /// </summary>
    public int ReservationTtlMinutes { get; init; } = 10;
}
