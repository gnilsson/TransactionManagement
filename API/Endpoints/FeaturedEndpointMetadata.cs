using System.Collections.Frozen;

namespace API.Endpoints;

public sealed class FeaturedEndpointMetadata
{
    public required string GroupName { get; init; }
    //public required string ForeignIdArgumentName { get; init; }
    public required CachingStrategyConfig CachingStrategy { get; init; }
    public required FrozenSet<string> AvailableSortOrders { get; init; }
}
