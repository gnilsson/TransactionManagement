using API.Endpoints;
using API.ExceptionHandling;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace API.Features;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class ResponseCaching
{
    public static class Tags
    {
        public const string GroupName = "gn:{0}";
        public const string GroupNameWithIdentifier = "gn:{0}_id:{1}";
    }

    public static class Keys
    {
        public const string Paginated = "gn:{0}_pn:{1}_ps:{2}_sb:{3}_sd:{4}_m:{5}";
        public const string PaginatedOnForeignId = "gn:{0}_id:{1}_pn:{2}_ps:{3}_sb:{4}_sd:{5}_m:{6}";
    }

    public sealed class StrategyConfig
    {
        [MemberNotNullWhen(false, nameof(ArgumentName))]
        public bool VariantIsDefault => Variant is StrategyVariant.Default;
        public StrategyVariant Variant { get; init; } = StrategyVariant.Default;
        public string? ArgumentName { get; init; }
    }

    public enum StrategyVariant : byte
    {
        Default,
        ForeignId
    }

    public record KeyAndTag(string Key, string Tag);

    public static KeyAndTag CreateKeyAndTag(HttpContext context)
    {
        var metadata = Routing.FeaturedEndpoints[context.Request.Path.Value!];
        var paginationQuery = (Pagination.Query)context.Items[Pagination.Defaults.QueryKey]!;

        if (metadata.CachingStrategy.VariantIsDefault)
        {
            var defaultCacheKey = string.Format(
                Keys.Paginated,
                metadata.GroupName,
                paginationQuery.PageNumber,
                paginationQuery.PageSize,
                paginationQuery.SortBy,
                paginationQuery.SortDirection,
                paginationQuery.Mode);

            var defaultTag = string.Format(Tags.GroupName, metadata.GroupName);

            return new(defaultCacheKey, defaultTag);
        }

        // Variant is CachingStrategy.StrategyVariant.ForeignId
        var foreignId = context.Request.Query[metadata.CachingStrategy.ArgumentName].ToString();

        var cacheKey = string.Format(
            Keys.PaginatedOnForeignId,
            metadata.GroupName,
            foreignId,
            paginationQuery.PageNumber,
            paginationQuery.PageSize,
            paginationQuery.SortBy,
            paginationQuery.SortDirection,
            paginationQuery.Mode);

        var tag = string.Format(Tags.GroupNameWithIdentifier, metadata.GroupName, foreignId);

        return new(cacheKey, tag);
    }

    // note:
    // the request parsing logic is here temporarily
    public static string CreateTag(GetEndpointMetadata metadata, byte[]? requestBodyBuffer)
    {
        if (metadata.CachingStrategy.VariantIsDefault)
        {
            return string.Format(Tags.GroupName, metadata.GroupName);
        }

        var reader = new Utf8JsonReader(requestBodyBuffer!);
        var foreignId = ReadForeignIdProperty(reader, metadata.CachingStrategy.ArgumentName);
        return string.Format(Tags.GroupNameWithIdentifier, metadata.GroupName, foreignId);
    }

    private static string ReadForeignIdProperty(Utf8JsonReader reader, string foreignIdArgumentName)
    {
        while (reader.Read())
        {
            if (reader.TokenType is JsonTokenType.PropertyName && reader.GetString() == foreignIdArgumentName)
            {
                reader.Read();
                return reader.GetString()!;
            }
        }
        ThrowHelper.Throw("The foreign ID property was not found in the request body.");
        return null!; // note: interesting that the DoesNotReturn attribute doesnt work when its applied last
    }
}
