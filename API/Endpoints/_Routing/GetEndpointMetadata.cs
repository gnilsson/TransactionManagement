using API.Features;
using System.Collections.Frozen;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace API.Endpoints;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public sealed class GetEndpointMetadata
{
    public required string GroupName { get; init; }
    public required ResponseCaching.StrategyConfig CachingStrategy { get; init; }
    public required FrozenSet<string> AvailableSortOrders { get; init; }
}
