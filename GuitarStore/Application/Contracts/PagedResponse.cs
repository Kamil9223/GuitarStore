namespace Application.Contracts;
public sealed class PagedResponse<T> where T : class
{
    public required IReadOnlyCollection<T> Items { get; init; }
    public required int Offset { get; init; }
    public required int Limit { get; init; }
    public required bool HasMoreItems { get; init; }
}
