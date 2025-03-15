#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace API.Features;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class ResponseCaching
{
    public static class Tags
    {
        public const string GroupNameWithIdentifier = "gn:{0}_id:{1}";
    }

    public static class Keys
    {
        public const string PaginatedOnForeignId = "gn:{0}_id:{1}_pn:{2}_ps:{3}_sb:{4}_sd:{5}_m:{6}";
    }

    public sealed class StrategyConfig
    {
        public StrategyVariant Variant { get; init; } = StrategyVariant.Default;
        public string? ArgumentName { get; init; }
    }

    public enum StrategyVariant : byte
    {
        Default,
        ForeignId
    }
}
