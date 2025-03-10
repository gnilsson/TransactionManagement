using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Data;

public sealed class Account : IIdentifiableEntity, ITemporalEntity, IRowVersionedEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public decimal Balance { get; set; }

    public ICollection<Transaction> Transactions { get; } = [];


    public DateTime CreatedAt { get; }

    public DateTime ModifiedAt { get; set; }

    [Required]
    public Guid UserId { get; init; }

    [ForeignKey(nameof(UserId))]
    public User User { get; } = default!;

    public long RowVersion { get; }
}
