using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Data;

public sealed class Transaction : IIdentifiableEntity, ITemporalEntity, IRowVersionedEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public required Guid AccountId { get; init; }

    [ForeignKey(nameof(AccountId))]
    public Account Account { get; } = default!;

    [Required]
    public required decimal Amount { get; init; }

    public long RowVersion { get; }

    public DateTime CreatedAt { get; }

    public DateTime ModifiedAt { get; }
}
