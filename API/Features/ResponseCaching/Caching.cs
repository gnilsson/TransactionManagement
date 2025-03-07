namespace API.Features.ResponseCaching;

public static class Caching
{
    public static class Tags
    {
        public const string GroupNameWithIdentifier = "gn:{0}_id:{1}";
    }

    public static class Keys
    {
        public const string PaginatedOnForeignId = "gn:{0}_id:{1}_pn:{2}_ps:{3}_sb:{4}_sd:{5}_m:{6}";
    }
}
