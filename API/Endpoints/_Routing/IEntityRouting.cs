using API.Features;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace API.Endpoints;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public interface IEntityRouting
{
    string GroupName { get; }
    IEnumerable<string>? SortOrders { get; }
    ResponseCaching.StrategyConfig CachingStrategy { get; }
    Type ResponseType { get; }
}
