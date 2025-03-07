namespace API.Endpoints;

public sealed class FeaturedEndpointMetadata
{
    public required string GroupName { get; init; }
    public required string ForeignIdArgumentName { get; init; }
    public required IReadOnlyDictionary<string, string> AvailableSortOrders { get; init; }
}
