#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace API.Data;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public interface IRowVersionedEntity
{
    long RowVersion { get; }
}
