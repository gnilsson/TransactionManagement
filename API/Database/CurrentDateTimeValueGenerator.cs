using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace API.Database;

public sealed class CurrentDateTimeValueGenerator : ValueGenerator<DateTime>
{
    public override bool GeneratesTemporaryValues => false;

    public override DateTime Next(EntityEntry entry)
    {
        return DateTime.UtcNow;
    }
}
