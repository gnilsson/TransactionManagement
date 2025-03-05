using API.Endpoints.AccountEndpoints;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Data;

public sealed class Transaction
{
    [Key]
    public Guid Id { get; }

    [Required]
    public required Guid AccountId { get; init; }

    [ForeignKey(nameof(AccountId))]
    public Account Account { get; set; } = default!;

    [Required]
    public required decimal Amount { get; init; }

    [Required]
    public DateTime CreatedAt { get; }

    public long RowVersion { get; }
}
