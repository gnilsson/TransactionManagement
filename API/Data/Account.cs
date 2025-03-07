using System.ComponentModel.DataAnnotations;

namespace API.Data;

public sealed class Account
{
    [Key]
    public Guid Id { get; }

    [Required]
    public decimal Balance { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = [];

    public long RowVersion { get; }
}
