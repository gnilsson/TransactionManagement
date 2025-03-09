using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Data;

public sealed class Account : IIdentifiableEntity, ITemporalEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public decimal Balance { get; set; }

    public ICollection<Transaction> Transactions { get; } = [];

    public long RowVersion { get; }

    public DateTime CreatedAt { get; }

    public DateTime ModifiedAt { get; }

    [Required]
    public Guid UserId { get; init; }

    [ForeignKey(nameof(UserId))]
    public User User { get; } = default!;
}
