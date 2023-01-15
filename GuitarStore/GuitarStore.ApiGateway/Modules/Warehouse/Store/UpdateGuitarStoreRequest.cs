namespace GuitarStore.ApiGateway.Modules.Warehouse.Store;

public class UpdateGuitarStoreRequest
{
    /// <summary>
    /// Guitar Store Location City
    /// </summary>
    public string? City { get; init; }

    /// <summary>
    /// Guitar Store Location Street + Number
    /// </summary>
    public string? Street { get; init; }

    /// <summary>
    /// Guitar Store Location Postal Code
    /// </summary>
    public string? PostalCode { get; init; }
}
